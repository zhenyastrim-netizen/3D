using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HitscanWeapon))]
[RequireComponent(typeof(WeaponReload))]
public class VoidcastShotgunEffect : MonoBehaviour
{
    [Header("Shard")]
    [SerializeField] private VoidShardProjectile shardPrefab;
    [SerializeField, Min(1)] private int maximumStoredShards = 30;
    [SerializeField, Range(0f, 2f)]
    private float returnDamageMultiplier = 0.45f;
    [SerializeField, Min(0.01f)] private float fallbackShardSize = 0.06f;

    [Header("Return flight")]
    [SerializeField, Min(0.1f)] private float returnSpeed = 24f;
    [SerializeField, Min(1f)] private float turnSpeed = 720f;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.35f;
    [SerializeField, Min(0.1f)] private float returnLifetime = 5f;

    [Header("Placement")]
    [SerializeField] private LayerMask shardSurfaceMask = ~0;
    [SerializeField, Min(0f)] private float surfaceOffset = 0.025f;
    [SerializeField, Min(0f)] private float groundSearchDistance = 3f;

    [Header("Mark")]
    [SerializeField] private GameObject markVisualPrefab;

    private readonly List<VoidShardProjectile> storedShards =
        new List<VoidShardProjectile>();

    private HitscanWeapon hitscanWeapon;
    private WeaponReload weaponReload;
    private PlayerDamageCalculator damageCalculator;
    private Transform markedTarget;
    private GameObject markVisual;

    public int StoredShardCount
    {
        get
        {
            RemoveDestroyedShards();
            return storedShards.Count;
        }
    }

    public Transform MarkedTarget => markedTarget;

    private void Awake()
    {
        hitscanWeapon = GetComponent<HitscanWeapon>();
        weaponReload = GetComponent<WeaponReload>();
        damageCalculator =
            GetComponentInParent<PlayerDamageCalculator>();
    }

    private void OnEnable()
    {
        if (hitscanWeapon != null)
        {
            hitscanWeapon.OnProjectileResolved +=
                HandleProjectileResolved;
        }

        if (weaponReload != null)
            weaponReload.OnReloadStarted += ReturnStoredShards;
    }

    private void OnDisable()
    {
        if (hitscanWeapon != null)
        {
            hitscanWeapon.OnProjectileResolved -=
                HandleProjectileResolved;
        }

        if (weaponReload != null)
            weaponReload.OnReloadStarted -= ReturnStoredShards;
    }

    private void OnDestroy()
    {
        foreach (VoidShardProjectile shard in storedShards)
        {
            if (shard != null)
                Destroy(shard.gameObject);
        }

        storedShards.Clear();
        ClearMark();
    }

    private void HandleProjectileResolved(
        HitscanProjectileResult result)
    {
        if (result.Damageable != null)
            MarkTarget(result.Damageable, result.HitTransform);

        StoreShard(result);
    }

    private void MarkTarget(
        IDamageable damageable,
        Transform fallbackTransform)
    {
        Component damageableComponent = damageable as Component;
        Transform newTarget = damageableComponent != null
            ? damageableComponent.transform
            : fallbackTransform;

        if (newTarget == null || newTarget == markedTarget)
            return;

        ClearMark();
        markedTarget = newTarget;

        if (markVisualPrefab != null)
        {
            markVisual = Instantiate(
                markVisualPrefab,
                markedTarget
            );

            markVisual.transform.localPosition = Vector3.zero;
            markVisual.transform.localRotation = Quaternion.identity;
        }
    }

    private void StoreShard(HitscanProjectileResult result)
    {
        RemoveDestroyedShards();

        int shardLimit = Mathf.Max(1, maximumStoredShards);

        while (storedShards.Count >= shardLimit)
        {
            VoidShardProjectile oldestShard = storedShards[0];
            storedShards.RemoveAt(0);

            if (oldestShard != null)
                Destroy(oldestShard.gameObject);
        }

        Vector3 position = FindShardPosition(result);
        Quaternion rotation = result.Direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(result.Direction)
            : Quaternion.identity;

        VoidShardProjectile shard = CreateShard(position, rotation);

        if (shard == null)
            return;

        shard.StoreDamage(
            result.BaseDamageParts,
            returnDamageMultiplier
        );

        storedShards.Add(shard);
    }

    private Vector3 FindShardPosition(
        HitscanProjectileResult result)
    {
        Vector3 position = result.EndPoint;

        if (groundSearchDistance > 0f)
        {
            RaycastHit[] groundHits = Physics.RaycastAll(
                result.EndPoint + Vector3.up * 0.25f,
                Vector3.down,
                groundSearchDistance,
                shardSurfaceMask,
                QueryTriggerInteraction.Ignore
            );

            System.Array.Sort(
                groundHits,
                (first, second) =>
                    first.distance.CompareTo(second.distance)
            );

            foreach (RaycastHit groundHit in groundHits)
            {
                if (groundHit.collider.transform.root == transform.root)
                    continue;

                if (groundHit.collider
                        .GetComponentInParent<IDamageable>() != null)
                {
                    continue;
                }

                position = groundHit.point +
                           groundHit.normal * surfaceOffset;
                return position;
            }
        }

        if (result.HitSomething)
        {
            position += result.SurfaceNormal * surfaceOffset;
        }

        return position;
    }

    private VoidShardProjectile CreateShard(
        Vector3 position,
        Quaternion rotation)
    {
        if (shardPrefab != null)
        {
            return Instantiate(
                shardPrefab,
                position,
                rotation
            );
        }

        GameObject fallback = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        fallback.name = "Void Shard (Runtime)";
        fallback.transform.SetPositionAndRotation(
            position,
            rotation
        );
        fallback.transform.localScale =
            Vector3.one * fallbackShardSize;

        Collider fallbackCollider = fallback.GetComponent<Collider>();

        if (fallbackCollider != null)
            fallbackCollider.enabled = false;

        return fallback.AddComponent<VoidShardProjectile>();
    }

    private void ReturnStoredShards()
    {
        RemoveDestroyedShards();

        if (!IsMarkedTargetAlive() || storedShards.Count == 0)
            return;

        Transform target = markedTarget;
        GameObject damageSource = damageCalculator != null
            ? damageCalculator.gameObject
            : gameObject;
        VoidShardProjectile[] shards = storedShards.ToArray();
        storedShards.Clear();

        foreach (VoidShardProjectile shard in shards)
        {
            if (shard == null)
                continue;

            shard.Launch(
                target,
                damageCalculator,
                damageSource,
                returnSpeed,
                turnSpeed,
                hitRadius,
                returnLifetime
            );
        }

        ClearMark();
    }

    private bool IsMarkedTargetAlive()
    {
        if (markedTarget == null)
            return false;

        EnemyHealth enemyHealth =
            markedTarget.GetComponentInParent<EnemyHealth>();

        return enemyHealth == null || !enemyHealth.IsDead;
    }

    private void RemoveDestroyedShards()
    {
        for (int i = storedShards.Count - 1; i >= 0; i--)
        {
            if (storedShards[i] == null)
                storedShards.RemoveAt(i);
        }
    }

    private void ClearMark()
    {
        if (markVisual != null)
            Destroy(markVisual);

        markVisual = null;
        markedTarget = null;
    }
}

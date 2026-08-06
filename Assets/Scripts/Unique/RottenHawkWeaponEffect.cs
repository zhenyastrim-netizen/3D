using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HitscanWeapon))]
public class RottenHawkWeaponEffect : MonoBehaviour
{
    [Header("Bird projectile")]
    [SerializeField] private RottenHawkProjectile birdProjectilePrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 38f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 3f;
    [SerializeField, Min(0.01f)] private float fallbackBirdScale = 0.18f;

    [Header("Decay per hit")]
    [SerializeField, Min(0f)] private float decayDamage;
    [SerializeField, Min(0f)] private float decayBuildup = 20f;

    [Header("Soft homing")]
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField, Min(0.1f)] private float acquisitionDistance = 22f;
    [SerializeField, Range(0f, 90f)] private float acquisitionAngle = 15f;
    [SerializeField, Min(0f)] private float homingTurnSpeed = 110f;
    [SerializeField, Min(0f)] private float homingDuration = 0.7f;
    [SerializeField, Min(0.02f)] private float reacquireInterval = 0.15f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private HitscanWeapon hitscanWeapon;
    private PlayerDamageCalculator damageCalculator;

    private void Awake()
    {
        hitscanWeapon = GetComponent<HitscanWeapon>();
        damageCalculator =
            GetComponentInParent<PlayerDamageCalculator>();
    }

    private void OnEnable()
    {
        if (hitscanWeapon != null)
        {
            hitscanWeapon.OnProjectileRequested +=
                HandleProjectileRequested;
        }
    }

    private void OnDisable()
    {
        if (hitscanWeapon != null)
        {
            hitscanWeapon.OnProjectileRequested -=
                HandleProjectileRequested;
        }
    }

    private bool HandleProjectileRequested(
        HitscanProjectileRequest request)
    {
        Quaternion rotation = Quaternion.LookRotation(
            request.Direction,
            Vector3.up
        );

        RottenHawkProjectile projectile =
            CreateProjectile(request.StartPoint, rotation);

        if (projectile == null)
            return false;

        projectile.Initialize(
            request.Direction,
            projectileSpeed,
            projectileLifetime,
            CreateDecayDamageParts(request.BaseDamageParts),
            damageCalculator,
            damageCalculator != null
                ? damageCalculator.gameObject
                : gameObject,
            targetMask,
            acquisitionDistance,
            acquisitionAngle,
            homingTurnSpeed,
            homingDuration,
            reacquireInterval,
            requireLineOfSight,
            obstacleMask
        );

        return true;
    }

    private RottenHawkProjectile CreateProjectile(
        Vector3 position,
        Quaternion rotation)
    {
        if (birdProjectilePrefab != null)
        {
            return Instantiate(
                birdProjectilePrefab,
                position,
                rotation
            );
        }

        return RottenHawkProjectile.CreateFallback(
            position,
            rotation,
            fallbackBirdScale
        );
    }

    private DamagePart[] CreateDecayDamageParts(
        DamagePart[] baseDamageParts)
    {
        int baseCount = baseDamageParts != null
            ? baseDamageParts.Length
            : 0;

        DamagePart[] result = new DamagePart[baseCount + 1];

        for (int i = 0; i < baseCount; i++)
            result[i] = baseDamageParts[i];

        result[baseCount] = new DamagePart(
            DamageType.Decay,
            decayDamage,
            decayBuildup
        );

        return result;
    }
}

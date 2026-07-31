using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

using System.Collections.Generic;
public class HitscanWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction fireAction;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CameraRecoil cameraRecoil;
    [SerializeField] private WeaponRecoil weaponRecoil;
    [SerializeField] private MuzzleFlash muzzleFlash;
    [SerializeField] private Tracer tracerPrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private ImpactEffect impactPrefab;
    [SerializeField] private WeaponAmmo ammo;
    [SerializeField] private WeaponReload reload;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerDamageCalculator damageCalculator;
    
    private WeaponData weaponData;
private bool isBursting;
    

    [Header("Weapon Settings")]
    [SerializeField] private DamagePart[] damageParts =
{
    new DamagePart(DamageType.Kinetic, 25f)
};
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawShotRay = true;

    private float nextFireTime;

    public void Initialize(
    Camera newPlayerCamera,
    CameraRecoil newCameraRecoil,
    WeaponData newWeaponData)
{
    playerCamera = newPlayerCamera;
    cameraRecoil = newCameraRecoil;
    weaponData = newWeaponData;

    if (weaponData != null)
        fireRate = weaponData.FireRate;
}

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        if (damageCalculator == null)
        {
            damageCalculator =
                GetComponentInParent<PlayerDamageCalculator>();
        }
    }

    private void OnEnable()
    {
        fireAction.Enable();
    }

    private void OnDisable()
{
    fireAction.Disable();
    StopAllCoroutines();
    isBursting = false;
}

    private void Update()
{
    if (weaponData == null || isBursting)
        return;

    float attackSpeed = playerStats != null
        ? playerStats.GetValue(StatType.AttackSpeed)
        : 1f;

    float finalFireRate =
        fireRate * Mathf.Max(0.01f, attackSpeed);

    if (Time.time < nextFireTime)
        return;

    switch (weaponData.FireMode)
    {
        case WeaponFireMode.SemiAutomatic:
            if (fireAction.WasPressedThisFrame())
                TrySingleShot(finalFireRate);
            break;

        case WeaponFireMode.Automatic:
            if (fireAction.IsPressed())
                TrySingleShot(finalFireRate);
            break;

        case WeaponFireMode.Burst:
            if (fireAction.WasPressedThisFrame())
            {
                StartCoroutine(
                    BurstRoutine(finalFireRate)
                );
            }
            break;
    }
}
private void TrySingleShot(float finalFireRate)
{
    if (!Shoot())
        return;

    nextFireTime =
        Time.time + 1f / finalFireRate;
}

private IEnumerator BurstRoutine(
    float finalFireRate)
{
    isBursting = true;

    int shots = Mathf.Max(
        1,
        weaponData.BurstSize
    );

    for (int i = 0; i < shots; i++)
    {
        if (!Shoot())
            break;

        if (i < shots - 1)
        {
            yield return new WaitForSeconds(
                weaponData.BurstDelay
            );
        }
    }

    nextFireTime =
        Time.time + 1f / finalFireRate;

    isBursting = false;
}

    private bool Shoot()
{
    if (reload != null && reload.IsReloading)
        return false;

    if (ammo == null || !ammo.CanShoot())
    {
        Debug.Log("Нет патронов");
        return false;
    }

    ammo.UseAmmo();

    muzzleFlash?.Play();
    cameraRecoil?.AddRecoil(2f, 0.5f);
    weaponRecoil?.AddRecoil();

    int projectileCount = Mathf.Max(
        1,
        weaponData.ProjectilesPerShot
    );

    for (int i = 0; i < projectileCount; i++)
        FireProjectile();

    return true;
}
private void FireProjectile()
{
    Ray aimRay = playerCamera.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );

    Vector2 spread =
        Random.insideUnitCircle *
        weaponData.SpreadAngle;

    Quaternion spreadRotation =
        Quaternion.Euler(
            -spread.y,
            spread.x,
            0f
        );

    Vector3 direction =
        spreadRotation * aimRay.direction;

    Ray shotRay = new Ray(
        aimRay.origin,
        direction
    );
    if (weaponData.PenetrationCount > 0)
{
    FirePenetratingProjectile(shotRay);
    return;
}

    Vector3 endPoint;

    if (Physics.Raycast(
            shotRay,
            out RaycastHit hit,
            range,
            hitMask,
            QueryTriggerInteraction.Ignore))
    {
        endPoint = hit.point;

        IDamageable damageable =
            hit.collider
                .GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            DamageInfo damageInfo =
                damageCalculator.CreateDamage(
                    damageParts,
                    AttackType.Ranged,
                    gameObject
                );

            damageable.TakeDamage(damageInfo);
            if (weaponData.RicochetCount > 0)
{
    FireRicochets(
        hit.point,
        hit.collider.transform.root
    );
}
            
        }
        

        if (impactPrefab != null)
        {
            ImpactEffect impact = Instantiate(
                impactPrefab,
                hit.point,
                Quaternion.identity
            );

            impact.Play(hit.point, hit.normal);
        }
    }
    else
    {
        endPoint =
            shotRay.origin +
            shotRay.direction * range;
    }

    if (tracerPrefab != null &&
        muzzlePoint != null)
    {
        Tracer tracer = Instantiate(
            tracerPrefab,
            muzzlePoint.position,
            Quaternion.identity
        );

        tracer.Setup(
            muzzlePoint.position,
            endPoint
        );
    }

    if (drawShotRay)
    {
        Debug.DrawRay(
            shotRay.origin,
            shotRay.direction * range,
            Color.green,
            1f
        );
    }
}
private void FireRicochets(
    Vector3 startPoint,
    Transform firstTarget)
{
    HashSet<Transform> hitTargets =
        new HashSet<Transform>();

    if (firstTarget != null)
        hitTargets.Add(firstTarget);

    hitTargets.Add(transform.root);

    Vector3 currentPoint = startPoint;
    float damageMultiplier = 1f;

    for (int i = 0;
         i < weaponData.RicochetCount;
         i++)
    {
        Collider nextTarget =
            FindNearestRicochetTarget(
                currentPoint,
                hitTargets
            );

        if (nextTarget == null)
            break;

        IDamageable damageable =
            nextTarget.GetComponentInParent<IDamageable>();

        if (damageable == null)
            break;

        Vector3 targetPoint =
            nextTarget.bounds.center;

        damageMultiplier *=
            weaponData.RicochetDamageMultiplier;

        DamagePart[] ricochetParts =
            CreateScaledDamageParts(
                damageMultiplier
            );

        DamageInfo damageInfo =
            damageCalculator.CreateDamage(
                ricochetParts,
                AttackType.Ranged,
                gameObject
            );

        damageable.TakeDamage(damageInfo);

        SpawnTracer(
            currentPoint,
            targetPoint
        );

        hitTargets.Add(
            nextTarget.transform.root
        );

        currentPoint = targetPoint;
    }
}
private Collider FindNearestRicochetTarget(
    Vector3 origin,
    HashSet<Transform> ignoredTargets)
{
    Collider[] colliders = Physics.OverlapSphere(
        origin,
        weaponData.RicochetRange,
        hitMask,
        QueryTriggerInteraction.Ignore
    );

    Collider nearest = null;
    float nearestDistance = float.MaxValue;

    foreach (Collider candidate in colliders)
    {
        Transform targetRoot =
            candidate.transform.root;

        if (ignoredTargets.Contains(targetRoot))
            continue;

        IDamageable damageable =
            candidate.GetComponentInParent<IDamageable>();

        if (damageable == null)
            continue;

        float distance =
            (candidate.bounds.center - origin)
            .sqrMagnitude;

        if (distance >= nearestDistance)
            continue;

        nearest = candidate;
        nearestDistance = distance;
    }

    return nearest;
}
private DamagePart[] CreateScaledDamageParts(
    float multiplier)
{
    DamagePart[] scaledParts =
        new DamagePart[damageParts.Length];

    for (int i = 0; i < damageParts.Length; i++)
    {
        DamagePart part = damageParts[i];

        part.damage *= multiplier;
        part.buildup *= multiplier;

        scaledParts[i] = part;
    }

    return scaledParts;
}
private void FirePenetratingProjectile(
    Ray shotRay)
{
    RaycastHit[] hits = Physics.RaycastAll(
        shotRay,
        range,
        hitMask,
        QueryTriggerInteraction.Ignore
    );

    System.Array.Sort(
        hits,
        (first, second) =>
            first.distance.CompareTo(second.distance)
    );

    Vector3 endPoint =
        shotRay.origin +
        shotRay.direction * range;

    int penetrationsRemaining =
        weaponData.PenetrationCount;

    HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    foreach (RaycastHit hit in hits)
    {
        endPoint = hit.point;

        IDamageable damageable =
            hit.collider
                .GetComponentInParent<IDamageable>();

        // Стена или другой недоступный объект
        // полностью останавливает пулю.
        if (damageable == null)
        {
            SpawnImpact(hit);
            break;
        }

        // Не наносим урон несколько раз,
        // если у врага несколько коллайдеров.
        if (!damagedTargets.Add(damageable))
            continue;

        DamageInfo damageInfo =
            damageCalculator.CreateDamage(
                damageParts,
                AttackType.Ranged,
                gameObject
            );

        damageable.TakeDamage(damageInfo);
        
        SpawnImpact(hit);

        if (penetrationsRemaining <= 0)
            break;

        penetrationsRemaining--;
    }

    SpawnTracer(
    muzzlePoint != null
        ? muzzlePoint.position
        : shotRay.origin,
    endPoint
);

    if (drawShotRay)
    {
        Debug.DrawRay(
            shotRay.origin,
            shotRay.direction * range,
            Color.cyan,
            1f
        );
    }
}
private void SpawnImpact(RaycastHit hit)
{
    if (impactPrefab == null)
        return;

    ImpactEffect impact = Instantiate(
        impactPrefab,
        hit.point,
        Quaternion.identity
    );

    impact.Play(hit.point, hit.normal);
}

private void SpawnTracer(
    Vector3 startPoint,
    Vector3 endPoint)
{
    if (tracerPrefab == null)
        return;

    Tracer tracer = Instantiate(
        tracerPrefab,
        startPoint,
        Quaternion.identity
    );

    tracer.Setup(
        startPoint,
        endPoint
    );
}
}
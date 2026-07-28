using UnityEngine;
using UnityEngine.InputSystem;

public class HitscanWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction fireAction;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Weapon Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private WeaponRecoil weaponRecoil;
    [SerializeField] private MuzzleFlash muzzleFlash;
    [SerializeField] private Tracer tracerPrefab;
[SerializeField] private Transform muzzlePoint;
[SerializeField] private ImpactEffect impactPrefab;
[SerializeField] private WeaponAmmo ammo;
[SerializeField] private WeaponReload reload;

    [Header("Debug")]
    [SerializeField] private bool drawShotRay = true;
    

    private float nextFireTime;
    [SerializeField]private CameraRecoil recoil;
    [SerializeField] private PlayerStats playerStats;

public void Initialize(
    Camera newPlayerCamera,
    CameraRecoil newCameraRecoil)
{
    playerCamera = newPlayerCamera;
    recoil = newCameraRecoil;
}

    private void Awake()
{
    if (playerCamera == null)
        playerCamera = Camera.main;

    if (playerStats == null)
        playerStats = GetComponentInParent<PlayerStats>();
}

    private void OnEnable()
    {
        fireAction.Enable();
    }

    private void OnDisable()
    {
        fireAction.Disable();
    }

    private void Update()
{
    float attackSpeed = playerStats != null
        ? playerStats.GetValue(StatType.AttackSpeed)
        : 1f;

    float finalFireRate = fireRate * Mathf.Max(0.01f, attackSpeed);

    if (fireAction.IsPressed() && Time.time >= nextFireTime)
    {
        nextFireTime = Time.time + 1f / finalFireRate;
        Shoot();
    }
}

    private void Shoot()
{
    if (reload.IsReloading)
    return;

if (!ammo.CanShoot())
{
    Debug.Log("Нет патронов");
    return;
}

ammo.UseAmmo();
    Vector3 endPoint;
    muzzleFlash?.Play();
    
    

    if (recoil != null)
    {
        recoil.AddRecoil(2f, 0.5f);
    }
    if (weaponRecoil != null)
{
    weaponRecoil.AddRecoil();
}

    Ray aimRay = playerCamera.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );
float damageMultiplier = playerStats != null
    ? playerStats.GetValue(StatType.RangedDamage)
    : 1f;

float finalDamage = damage * damageMultiplier;

float criticalChance = playerStats != null
    ? playerStats.GetValue(StatType.CriticalChance)
    : 0f;

bool isCritical = Random.value < criticalChance;

if (isCritical)
{
    float criticalDamage = playerStats.GetValue(
        StatType.CriticalDamage
    );

    finalDamage *= criticalDamage;
}
    if (Physics.Raycast(
    aimRay,
    out RaycastHit hit,
    range,
    hitMask,
    QueryTriggerInteraction.Ignore))
{
    endPoint = hit.point;

    IDamageable damageable =
        hit.collider.GetComponentInParent<IDamageable>();

    damageable?.TakeDamage(damage);
    damageable?.TakeDamage(damage);

if(impactPrefab != null)
{
    ImpactEffect impact = Instantiate(
        impactPrefab,
        hit.point,
        Quaternion.identity
    );

    impact.Play(
        hit.point,
        hit.normal
    );
}
}


    else
{
    endPoint = aimRay.origin + aimRay.direction * range;
}


if (tracerPrefab != null && muzzlePoint != null)
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
Debug.Log("Tracer created");


if (drawShotRay)
{
    Debug.DrawRay(
        aimRay.origin,
        aimRay.direction * range,
        Color.green,
        1f
    );
}
    {
       
    
}
}
}

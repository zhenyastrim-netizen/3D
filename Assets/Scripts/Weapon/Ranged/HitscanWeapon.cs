using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Weapon Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawShotRay = true;

    private float nextFireTime;

    public void Initialize(
        Camera newPlayerCamera,
        CameraRecoil newCameraRecoil)
    {
        playerCamera = newPlayerCamera;
        cameraRecoil = newCameraRecoil;
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
    }

    private void Update()
    {
        float attackSpeed = playerStats != null
            ? playerStats.GetValue(StatType.AttackSpeed)
            : 1f;

        float finalFireRate =
            fireRate * Mathf.Max(0.01f, attackSpeed);

        if (fireAction.IsPressed() &&
            Time.time >= nextFireTime)
        {
            Shoot();

            nextFireTime =
                Time.time + 1f / finalFireRate;
        }
    }

    private void Shoot()
    {
        if (reload != null && reload.IsReloading)
            return;

        if (ammo == null || !ammo.CanShoot())
        {
            Debug.Log("Нет патронов");
            return;
        }

        ammo.UseAmmo();
        muzzleFlash?.Play();

        cameraRecoil?.AddRecoil(2f, 0.5f);
        weaponRecoil?.AddRecoil();

        Ray aimRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 endPoint;

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

            if (damageable != null)
            {
                DamageInfo damageInfo =
                    damageCalculator.CreateDamage(
                        damage,
                        DamageType.Ranged,
                        gameObject
                    );

                damageable.TakeDamage(damageInfo);
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
                aimRay.origin + aimRay.direction * range;
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

        if (drawShotRay)
        {
            Debug.DrawRay(
                aimRay.origin,
                aimRay.direction * range,
                Color.green,
                1f
            );
        }
    }
}
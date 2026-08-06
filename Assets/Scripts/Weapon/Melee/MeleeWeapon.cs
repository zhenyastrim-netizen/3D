using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction attackAction;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerDamageCalculator damageCalculator;

    [Header("Attack")]
    [SerializeField] private DamagePart[] damageParts =
    {
        new DamagePart(DamageType.Kinetic, 35f)
    };
    [SerializeField] private float range = 3f;
    [SerializeField] private float hitRadius = 0.6f;
    [SerializeField] private float attacksPerSecond = 2f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawAttackRay = true;

    private float nextAttackTime;
[SerializeField] private MeleeWeaponVisual weaponVisual;
[SerializeField] private CameraRecoil cameraRecoil;
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
    public void Initialize(
    Camera camera,
    PlayerStats stats,
    PlayerDamageCalculator calculator,
    CameraRecoil recoil,
    WeaponData weaponData)
{
    if (camera != null)
        playerCamera = camera;

    if (stats != null)
        playerStats = stats;

    if (calculator != null)
        damageCalculator = calculator;

    if (recoil != null)
        cameraRecoil = recoil;

    if (weaponData == null)
        return;

    damageParts = weaponData.GetDamageParts();
    range = weaponData.MeleeRange;
    hitRadius = weaponData.MeleeHitRadius;
    attacksPerSecond =
        weaponData.MeleeAttacksPerSecond;
}

    private void OnEnable()
    {
        attackAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
    }

    private void Update()
    {
        if (!attackAction.WasPressedThisFrame())
            return;

        if (Time.time < nextAttackTime)
            return;

        Attack();
    }

    private void Attack()
    {
        weaponVisual?.PlayAttack();
        float attackSpeed = playerStats != null
            ? playerStats.GetValue(StatType.AttackSpeed)
            : 1f;

        float finalAttackRate =
            attacksPerSecond * Mathf.Max(0.01f, attackSpeed);

        nextAttackTime =
            Time.time + 1f / finalAttackRate;

        Ray attackRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.SphereCast(
            attackRay,
            hitRadius,
            out RaycastHit hit,
            range,
            hitMask,
            QueryTriggerInteraction.Ignore))
        {
            IDamageable damageable =
                hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
DamageInfo damageInfo =
    damageCalculator.CreateDamage(
        damageParts,
        AttackType.Melee,
        gameObject
    );

                damageable.TakeDamage(damageInfo);
            }
            cameraRecoil?.AddRecoil(1.5f, 0.4f);

            Debug.Log($"Мили-попадание: {hit.collider.name}");
        }

        if (drawAttackRay)
        {
            Debug.DrawRay(
                attackRay.origin,
                attackRay.direction * range,
                Color.red,
                0.5f
            );
        }
    }
}
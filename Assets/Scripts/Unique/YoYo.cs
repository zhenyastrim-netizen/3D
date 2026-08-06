using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class YoYoWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction attackAction;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform handAnchor;
    [SerializeField] private Transform yoYoTransform;
    [SerializeField] private LineRenderer stringRenderer;
    [SerializeField] private GameObject chargedVisual;
    [SerializeField] private ParticleSystem chargedParticles;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerDamageCalculator damageCalculator;
    [SerializeField] private PlayerParry playerParry;
    [SerializeField] private PlayerCombatEvents combatEvents;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float maxRange = 9f;
    [SerializeField, Min(0.1f)] private float launchSpeed = 24f;
    [SerializeField, Min(0.1f)] private float followSpeed = 35f;
    [SerializeField, Min(0.1f)] private float returnSpeed = 30f;
    [SerializeField, Min(0f)] private float surfaceOffset = 0.15f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField, Min(0f)] private float rotationSpeed = 1080f;

    [Header("Damage")]
    [SerializeField, Min(0f)] private float kineticDamage = 12f;
    [SerializeField] private DamageType baseDamageType = DamageType.Kinetic;
    [SerializeField, Min(0f)] private float lightningDamage = 8f;
    [SerializeField, Min(0.01f)] private float damageTicksPerSecond = 4f;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.7f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Unique Charge")]
    [SerializeField, Min(0.01f)] private float chargeDuration = 3f;
    [SerializeField, Min(0.01f)] private float instantChargeWindow = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawHitRadius = true;

    private readonly HashSet<IDamageable> damagedThisTick =
        new HashSet<IDamageable>();
    private readonly RaycastHit[] aimHits = new RaycastHit[16];

    private Vector3 restPositionInAnchor;
    private Quaternion restRotationInAnchor;
    private float chargeElapsed;
    private float instantChargeExpiresAt = float.NegativeInfinity;
    private float nextDamageTime;
    private bool isDeployed;
    private bool isReturning;
    private bool isCharged;
    private bool hasReachedAimPoint;
    private bool subscribedToParry;

    public bool IsDeployed => isDeployed;
    public bool IsCharged => isCharged;
    public float ChargeProgress => isCharged
        ? 1f
        : Mathf.Clamp01(chargeElapsed / Mathf.Max(0.01f, chargeDuration));
    public float InstantChargeWindowRemaining =>
        Mathf.Max(0f, instantChargeExpiresAt - Time.time);

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (handAnchor == null)
            handAnchor = transform;

        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        if (damageCalculator == null)
            damageCalculator = GetComponentInParent<PlayerDamageCalculator>();

        if (playerParry == null)
            playerParry = GetComponentInParent<PlayerParry>();

        if (combatEvents == null)
            combatEvents = GetComponentInParent<PlayerCombatEvents>();

        RememberRestPose();

        if (stringRenderer != null)
            stringRenderer.useWorldSpace = true;

        SetCharged(false);
        SetStringVisible(false);
    }

    public void Initialize(
        Camera camera,
        PlayerStats stats,
        PlayerDamageCalculator calculator,
        PlayerParry parry,
        WeaponData weaponData)
    {
        UnsubscribeFromParry();

        if (camera != null)
            playerCamera = camera;

        if (stats != null)
            playerStats = stats;

        if (calculator != null)
            damageCalculator = calculator;

        if (parry != null)
            playerParry = parry;

        if (weaponData != null)
        {
            kineticDamage = weaponData.Damage;
            baseDamageType = weaponData.DamageType;
            maxRange = weaponData.MeleeRange;
            hitRadius = weaponData.MeleeHitRadius;
            damageTicksPerSecond = weaponData.MeleeAttacksPerSecond;
        }

        combatEvents = GetComponentInParent<PlayerCombatEvents>();
        SubscribeToParry();
    }

    private void OnEnable()
    {
        attackAction.Enable();
        SubscribeToParry();
    }

    private void OnDisable()
    {
        attackAction.Disable();
        UnsubscribeFromParry();
        ResetYoYo();
    }

    private void Update()
    {
        if (yoYoTransform == null || playerCamera == null)
            return;

        if (!isDeployed && attackAction.WasPressedThisFrame())
            Deploy();

        if (!isDeployed)
            return;

        RotateYoYo();

        if (!isReturning && attackAction.IsPressed())
        {
            HoldAtAimPoint();
            UpdateCharge();
            TryDamageTick();
        }
        else
        {
            isReturning = true;
            ReturnToHand();
        }

        UpdateString();
    }

    private void Deploy()
    {
        isDeployed = true;
        isReturning = false;
        hasReachedAimPoint = false;
        chargeElapsed = 0f;
        nextDamageTime = Time.time;
        SetStringVisible(true);

        if (Time.time <= instantChargeExpiresAt)
        {
            instantChargeExpiresAt = float.NegativeInfinity;
            SetCharged(true);
        }
        else
        {
            SetCharged(false);
        }
    }

    private void HoldAtAimPoint()
    {
        Vector3 targetPoint = GetAimPoint();
        float speed = hasReachedAimPoint ? followSpeed : launchSpeed;

        yoYoTransform.position = Vector3.MoveTowards(
            yoYoTransform.position,
            targetPoint,
            speed * Time.deltaTime
        );

        if (!hasReachedAimPoint &&
            (yoYoTransform.position - targetPoint).sqrMagnitude <= 0.0025f)
        {
            hasReachedAimPoint = true;
        }
    }

    private Vector3 GetAimPoint()
    {
        Ray aimRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        int hitCount = Physics.RaycastNonAlloc(
            aimRay,
            aimHits,
            maxRange,
            aimMask,
            QueryTriggerInteraction.Ignore
        );

        float nearestDistance = maxRange;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = aimHits[i];

            if (hit.collider == null ||
                hit.collider.transform.root == transform.root ||
                hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = hit.distance;
        }

        float targetDistance = Mathf.Max(
            0f,
            nearestDistance - surfaceOffset
        );

        return aimRay.origin + aimRay.direction * targetDistance;
    }

    private void ReturnToHand()
    {
        Vector3 restPosition = GetRestWorldPosition();

        yoYoTransform.position = Vector3.MoveTowards(
            yoYoTransform.position,
            restPosition,
            returnSpeed * Time.deltaTime
        );

        if ((yoYoTransform.position - restPosition).sqrMagnitude > 0.0004f)
            return;

        ResetYoYo();
    }

    private void UpdateCharge()
    {
        if (isCharged)
            return;

        chargeElapsed += Time.deltaTime;

        if (chargeElapsed >= chargeDuration)
            SetCharged(true);
    }

    private void TryDamageTick()
    {
        if (Time.time < nextDamageTime)
            return;

        float attackSpeed = playerStats != null
            ? playerStats.GetValue(StatType.AttackSpeed)
            : 1f;

        float finalTickRate =
            damageTicksPerSecond * Mathf.Max(0.01f, attackSpeed);

        nextDamageTime = Time.time + 1f / finalTickRate;
        DamageTargets();
    }

    private void DamageTargets()
    {
        if (damageCalculator == null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            yoYoTransform.position,
            hitRadius,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        damagedThisTick.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null || !damagedThisTick.Add(damageable))
                continue;

            DamageTarget(hit, damageable);
        }
    }

    private void DamageTarget(Collider hit, IDamageable damageable)
    {
        DamagePart[] parts = isCharged
            ? new[]
            {
                new DamagePart(baseDamageType, kineticDamage),
                new DamagePart(DamageType.Lightning, lightningDamage)
            }
            : new[]
            {
                new DamagePart(baseDamageType, kineticDamage)
            };

        DamageInfo damageInfo = damageCalculator.CreateDamage(
            parts,
            AttackType.Melee,
            gameObject
        );

        EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
        bool wasAlive = enemy != null && !enemy.IsDead;

        damageable.TakeDamage(damageInfo);

        if (isCharged)
            TriggerLightningChain(hit, damageInfo);

        if (combatEvents != null && enemy != null)
        {
            combatEvents.ReportEnemyHit(
                new CombatHitInfo(
                    enemy.gameObject,
                    damageInfo.TotalDamage,
                    AttackType.Melee,
                    damageInfo.IsCritical,
                    false,
                    wasAlive && enemy.IsDead
                )
            );
        }
    }

    private void TriggerLightningChain(Collider hit, DamageInfo damageInfo)
    {
        LightningChainController chain =
            hit.GetComponentInParent<LightningChainController>();

        if (chain == null || damageInfo.Parts == null)
            return;

        foreach (DamagePart part in damageInfo.Parts)
        {
            if (part.damageType != DamageType.Lightning)
                continue;

            chain.TriggerChain(part, damageInfo);
            return;
        }
    }

    private void HandleSuccessfulParry(GameObject attacker)
    {
        instantChargeExpiresAt = Time.time + instantChargeWindow;

        if (!isDeployed || isReturning)
            return;

        instantChargeExpiresAt = float.NegativeInfinity;
        SetCharged(true);
    }

    private void SetCharged(bool charged)
    {
        isCharged = charged;

        if (charged)
            chargeElapsed = chargeDuration;

        if (charged)
        {
            if (chargedVisual != null)
                chargedVisual.SetActive(true);

            if (chargedParticles != null)
                chargedParticles.Play();
        }
        else
        {
            if (chargedParticles != null)
            {
                chargedParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            if (chargedVisual != null)
                chargedVisual.SetActive(false);
        }
    }

    private void RotateYoYo()
    {
        if (rotationAxis.sqrMagnitude <= 0.0001f || rotationSpeed <= 0f)
            return;

        yoYoTransform.Rotate(
            rotationAxis.normalized,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private void RememberRestPose()
    {
        if (yoYoTransform == null || handAnchor == null)
            return;

        restPositionInAnchor = handAnchor.InverseTransformPoint(
            yoYoTransform.position
        );

        restRotationInAnchor =
            Quaternion.Inverse(handAnchor.rotation) * yoYoTransform.rotation;
    }

    private Vector3 GetRestWorldPosition()
    {
        return handAnchor != null
            ? handAnchor.TransformPoint(restPositionInAnchor)
            : transform.position;
    }

    private void ResetYoYo()
    {
        isDeployed = false;
        isReturning = false;
        hasReachedAimPoint = false;
        chargeElapsed = 0f;
        damagedThisTick.Clear();
        SetCharged(false);
        SetStringVisible(false);

        if (yoYoTransform == null || handAnchor == null)
            return;

        yoYoTransform.position = GetRestWorldPosition();
        yoYoTransform.rotation =
            handAnchor.rotation * restRotationInAnchor;
    }

    private void UpdateString()
    {
        if (stringRenderer == null || handAnchor == null || yoYoTransform == null)
            return;

        stringRenderer.positionCount = 2;
        stringRenderer.SetPosition(0, handAnchor.position);
        stringRenderer.SetPosition(1, yoYoTransform.position);
    }

    private void SetStringVisible(bool visible)
    {
        if (stringRenderer != null)
            stringRenderer.enabled = visible;
    }

    private void SubscribeToParry()
    {
        if (subscribedToParry || playerParry == null || !isActiveAndEnabled)
            return;

        playerParry.OnParrySucceeded += HandleSuccessfulParry;
        subscribedToParry = true;
    }

    private void UnsubscribeFromParry()
    {
        if (!subscribedToParry || playerParry == null)
            return;

        playerParry.OnParrySucceeded -= HandleSuccessfulParry;
        subscribedToParry = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawHitRadius || yoYoTransform == null)
            return;

        Gizmos.color = isCharged ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(yoYoTransform.position, hitRadius);
    }
#endif
}
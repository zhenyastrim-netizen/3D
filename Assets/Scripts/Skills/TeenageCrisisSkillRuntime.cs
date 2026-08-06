using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCombatEvents))]
public class TeenageCrisisSkillRuntime : MonoBehaviour
{
    private sealed class DamageSnapshot
    {
        public DamagePart[] Parts { get; }
        public AttackType AttackType { get; }

        public DamageSnapshot(
            DamagePart[] parts,
            AttackType attackType)
        {
            Parts = parts;
            AttackType = attackType;
        }
    }

    private PlayerStats playerStats;
    private PlayerCombatEvents combatEvents;
    private WeaponManager weaponManager;

    private int rank;
    private float damageTakenIncrease;
    private float moveSpeedBonus;
    private float outgoingDamageBonus;
    private float criticalChanceBonus;

    private int maxStacks = 6;
    private float sameWeaponResetTime = 3f;
    private float volleyInterval = 1f;
    private float bulletDamageMultiplier = 0.35f;
    private GameObject bulletPrefab;
    private float bulletSpeed = 15f;
    private float bulletLifetime = 2f;
    private float bulletSpawnHeight = 1f;

    private readonly List<DamageSnapshot> snapshots =
        new List<DamageSnapshot>();

    private int currentWeaponKey;
    private bool hasCurrentWeapon;
    private float sameWeaponStartTime;
    private float nextVolleyTime;

    public int CurrentStacks => snapshots.Count;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        combatEvents = GetComponent<PlayerCombatEvents>();
        weaponManager = GetComponentInChildren<WeaponManager>(true);
    }

    private void OnEnable()
    {
        if (combatEvents == null)
            combatEvents = GetComponent<PlayerCombatEvents>();

        combatEvents.OnEnemyHit += HandleEnemyHit;

        if (rank > 0)
            RefreshModifiers();
    }

    private void OnDisable()
    {
        if (combatEvents != null)
            combatEvents.OnEnemyHit -= HandleEnemyHit;

        if (playerStats != null)
            playerStats.RemoveModifiersFromSource(this);

        ClearUncertainty();
    }

    private void Update()
    {
        if (CurrentStacks <= 0 || Time.time < nextVolleyTime)
            return;

        FireUncertaintyVolley();
        nextVolleyTime = Time.time + volleyInterval;
    }

    public void Configure(
        int newRank,
        float newDamageTakenIncrease,
        float newMoveSpeedBonus,
        float newOutgoingDamageBonus,
        float newCriticalChanceBonus,
        int newMaxStacks,
        float newSameWeaponResetTime,
        float newVolleyInterval,
        float newBulletDamageMultiplier,
        GameObject newBulletPrefab,
        float newBulletSpeed,
        float newBulletLifetime,
        float newBulletSpawnHeight)
    {
        rank = Mathf.Max(1, newRank);
        damageTakenIncrease = Mathf.Max(0f, newDamageTakenIncrease);
        moveSpeedBonus = Mathf.Max(0f, newMoveSpeedBonus);
        outgoingDamageBonus = Mathf.Max(0f, newOutgoingDamageBonus);
        criticalChanceBonus = Mathf.Clamp01(newCriticalChanceBonus);

        maxStacks = Mathf.Max(1, newMaxStacks);
        sameWeaponResetTime = Mathf.Max(0.1f, newSameWeaponResetTime);
        volleyInterval = Mathf.Max(0.05f, newVolleyInterval);
        bulletDamageMultiplier = Mathf.Max(0f, newBulletDamageMultiplier);
        bulletPrefab = newBulletPrefab;
        bulletSpeed = Mathf.Max(0f, newBulletSpeed);
        bulletLifetime = Mathf.Max(0.1f, newBulletLifetime);
        bulletSpawnHeight = Mathf.Max(0f, newBulletSpawnHeight);

        while (snapshots.Count > maxStacks)
            snapshots.RemoveAt(0);

        RefreshModifiers();
    }

    private void HandleEnemyHit(CombatHitInfo hitInfo)
    {
        if (hitInfo.IsSecondary)
            return;

        int weaponKey = ResolveWeaponKey(hitInfo);

        if (!hasCurrentWeapon)
        {
            hasCurrentWeapon = true;
            currentWeaponKey = weaponKey;
            sameWeaponStartTime = Time.time;
            return;
        }

        if (weaponKey == currentWeaponKey)
        {
            if (Time.time - sameWeaponStartTime >=
                sameWeaponResetTime)
            {
                ClearUncertainty();
                hasCurrentWeapon = true;
                currentWeaponKey = weaponKey;
                sameWeaponStartTime = Time.time;
            }

            return;
        }

        currentWeaponKey = weaponKey;
        sameWeaponStartTime = Time.time;
        AddUncertaintyStack(hitInfo);
    }

    private int ResolveWeaponKey(CombatHitInfo hitInfo)
    {
        if (weaponManager == null)
            weaponManager = GetComponentInChildren<WeaponManager>(true);

        WeaponInstance weapon =
            weaponManager != null
                ? weaponManager.CurrentWeaponInstance
                : null;

        if (weapon != null)
            return RuntimeHelpers.GetHashCode(weapon);

        if (hitInfo.Source != null)
            return hitInfo.Source.GetInstanceID();

        return 1000 + (int)hitInfo.AttackType;
    }

    private void AddUncertaintyStack(CombatHitInfo hitInfo)
    {
        DamagePart[] parts = CreateSnapshotParts(hitInfo);

        if (snapshots.Count >= maxStacks)
            snapshots.RemoveAt(0);

        snapshots.Add(
            new DamageSnapshot(parts, hitInfo.AttackType)
        );

        if (snapshots.Count == 1)
            nextVolleyTime = Time.time + volleyInterval;
    }

    private DamagePart[] CreateSnapshotParts(CombatHitInfo hitInfo)
    {
        DamagePart[] sourceParts = hitInfo.DamageParts;

        if (sourceParts == null || sourceParts.Length == 0)
        {
            return new[]
            {
                new DamagePart(
                    DamageType.Kinetic,
                    hitInfo.DamageDealt * bulletDamageMultiplier
                )
            };
        }

        DamagePart[] result = new DamagePart[sourceParts.Length];

        for (int i = 0; i < sourceParts.Length; i++)
        {
            DamagePart part = sourceParts[i];
            part.damage *= bulletDamageMultiplier;
            part.buildup *= bulletDamageMultiplier;
            result[i] = part;
        }

        return result;
    }

    private void FireUncertaintyVolley()
    {
        int bulletCount = snapshots.Count;

        if (bulletCount <= 0)
            return;

        Vector3 origin =
            transform.position +
            Vector3.up * bulletSpawnHeight;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f * i / bulletCount;
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) *
                transform.forward;

            SpawnBullet(origin, direction, snapshots[i]);
        }
    }

    private void SpawnBullet(
        Vector3 origin,
        Vector3 direction,
        DamageSnapshot snapshot)
    {
        GameObject bulletObject;

        if (bulletPrefab != null)
        {
            bulletObject = Instantiate(
                bulletPrefab,
                origin,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            bulletObject = GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

            bulletObject.name = "Uncertainty Bullet";
            bulletObject.transform.position = origin;
            bulletObject.transform.localScale =
                Vector3.one * 0.2f;

            bulletObject.AddComponent<Rigidbody>();
        }

        UncertaintyProjectile projectile =
            bulletObject.GetComponent<UncertaintyProjectile>();

        if (projectile == null)
            projectile = bulletObject.AddComponent<UncertaintyProjectile>();

        projectile.Initialize(
            direction,
            bulletSpeed,
            bulletLifetime,
            snapshot.Parts,
            snapshot.AttackType,
            gameObject
        );
    }

    private void RefreshModifiers()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerStats.RemoveModifiersFromSource(this);

        AddPercent(StatType.DamageTakenMultiplier, damageTakenIncrease);
        AddPercent(StatType.MoveSpeed, moveSpeedBonus);
        AddPercent(StatType.MeleeDamage, outgoingDamageBonus);
        AddPercent(StatType.RangedDamage, outgoingDamageBonus);
        AddPercent(StatType.MagicDamage, outgoingDamageBonus);

        playerStats.AddModifier(
            new StatModifier(
                StatType.CriticalChance,
                StatModifierType.Flat,
                criticalChanceBonus * rank,
                this
            )
        );
    }

    private void AddPercent(StatType statType, float value)
    {
        playerStats.AddModifier(
            new StatModifier(
                statType,
                StatModifierType.Percent,
                value * rank,
                this
            )
        );
    }

    private void ClearUncertainty()
    {
        snapshots.Clear();
        hasCurrentWeapon = false;
        currentWeaponKey = 0;
        sameWeaponStartTime = 0f;
        nextVolleyTime = 0f;
    }
}

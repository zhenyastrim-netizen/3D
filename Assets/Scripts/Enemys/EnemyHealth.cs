using UnityEngine;
using System;
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Damage multipliers")]
    [Header("Defense")]
[SerializeField, Min(0f)]
private float kineticDefense;

[SerializeField, Min(0f)]
private float spiritualDefense;
public event Action<float, DamageType, bool, bool> OnDamageReceived;
public event Action<EnemyHealth> OnDied;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0f;
    private EnemyStatusController statusController;

    private float currentHealth;
    private bool isDead;
    private float kineticDefenseMultiplier = 1f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    private LightningChainController lightningChain;
    [Header("Experience")]
[SerializeField, Min(0)]
private int experienceReward = 25;

private GameObject lastDamageSource;

    private void Awake()
    {
        lightningChain =
    GetComponent<LightningChainController>();
        statusController =
    GetComponent<EnemyStatusController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
{
    if (damageInfo.Source != null)
    lastDamageSource = damageInfo.Source;
    if (isDead || damageInfo.Parts == null)
        return;

    float finalDamage = 0f;

foreach (DamagePart part in damageInfo.Parts)
{
    float partDamage = CalculateDamage(part);

    finalDamage += partDamage;

    statusController?.ApplyBuildup(
        part,
        damageInfo.Source
    );

    if (part.damageType == DamageType.Lightning &&
        !damageInfo.IsSecondary)
    {
        lightningChain?.TriggerChain(
            part,
            damageInfo
        );
    }

    if (partDamage > 0f)
    {
        OnDamageReceived?.Invoke(
            partDamage,
            part.damageType,
            damageInfo.IsCritical,
            damageInfo.IsSecondary
        );
    }
}

    if (finalDamage <= 0f)
        return;

    float healthBeforeHit = currentHealth;

    currentHealth = Mathf.Max(
        currentHealth - finalDamage,
        0f
    );

    float damageDealt = healthBeforeHit - currentHealth;
    bool killedByHit = currentHealth <= 0f;

    PlayerCombatEvents combatEvents = damageInfo.Source != null
        ? damageInfo.Source.GetComponentInParent<PlayerCombatEvents>()
        : null;

    combatEvents?.ReportEnemyHit(
        new CombatHitInfo(
            gameObject,
            damageDealt,
            damageInfo.AttackType,
            damageInfo.IsCritical,
            damageInfo.IsSecondary,
            killedByHit,
            damageInfo.Source,
            damageInfo.Parts
        )
    );

    Debug.Log(
        $"{gameObject.name} получил {finalDamage:F1} урона. " +
        $"Атака: {damageInfo.AttackType}. " +
        $"Крит: {damageInfo.IsCritical}. " +
        $"HP: {currentHealth:F1}/{maxHealth:F1}"
    );

    if (currentHealth <= 0f)
        Die();
}
private float CalculateDamage(DamagePart part)
{
    switch (part.damageType)
    {
        case DamageType.Kinetic:
            return ApplyDefense(
                part.damage,
                kineticDefense * kineticDefenseMultiplier
            );

        case DamageType.Spiritual:
        case DamageType.Fire:
        case DamageType.Lightning:
        case DamageType.Frost:
        case DamageType.Decay:
            return ApplyDefense(
                part.damage,
                spiritualDefense
            );

        case DamageType.Holy:
        case DamageType.Cursed:
        default:
            return part.damage;
    }
}
public void SetKineticDefenseMultiplier(
    float multiplier)
{
    kineticDefenseMultiplier =
        Mathf.Clamp01(multiplier);
}

private float ApplyDefense(
    float damage,
    float defense)
{
    defense = Mathf.Max(0f, defense);

    return damage * (100f / (100f + defense));
}
   

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );
    }

    private void Die()
    {
        
        if (isDead)
            return;

        isDead = true;

        OnDied?.Invoke(this);

        Debug.Log($"{gameObject.name} умер");

        EnemyBrain brain = GetComponent<EnemyBrain>();
        brain?.SetDead();
GiveExperience();
        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
            
    }
    private void GiveExperience()
{
    if (lastDamageSource == null ||
        experienceReward <= 0)
    {
        return;
    }

    PlayerExperience experience =
        lastDamageSource.GetComponentInParent<PlayerExperience>();

    if (experience == null)
        return;

    experience.AddExperience(experienceReward);

    Debug.Log(
        $"Получено опыта: {experienceReward}",
        this
    );
}
}

using UnityEngine;
using System;
using System.Collections;
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerStats playerStats;
    private PlayerCombatEvents combatEvents;
    

    private float currentHealth;
    private float maxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public event Action<float, float> OnHealthChanged;
    [Header("Health Gate")]
[SerializeField, Range(0f, 1f)]
private float healthGateThreshold = 0.5f;

[SerializeField, Min(0f)]
private float invulnerabilityDuration = 0.75f;

private bool healthGateAvailable = true;
private bool isInvulnerable;
public event Action<float> OnDamaged;
public event Action OnHealthGateTriggered;
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        combatEvents = GetComponent<PlayerCombatEvents>();
    }

    private void OnEnable()
    {
        playerStats.OnStatChanged += HandleStatChanged;
    }
    private void Update()
{
    if (IsDead || currentHealth >= maxHealth)
        return;

    float regeneration =
        playerStats.GetValue(StatType.HealthRegeneration);

    if (regeneration <= 0f)
        return;

    Heal(regeneration * Time.deltaTime);
}

    private void Start()
    {
        maxHealth = playerStats.GetValue(StatType.MaxHealth);
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDisable()
    {
        playerStats.OnStatChanged -= HandleStatChanged;
    }

    private void HandleStatChanged(StatType statType, float newValue)
    {
        if (statType != StatType.MaxHealth)
            return;

        float healthDifference = newValue - maxHealth;

        maxHealth = Mathf.Max(1f, newValue);
        currentHealth = Mathf.Clamp(
            currentHealth + healthDifference,
            0f,
            maxHealth
        );
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

   public void TakeDamage(DamageInfo damageInfo)
{
    if (IsDead ||
        isInvulnerable ||
        damageInfo.Parts == null)
    {
        return;
    }

    float finalDamage = 0f;

    foreach (DamagePart part in damageInfo.Parts)
    {
        finalDamage += CalculateDamage(part);
    }

    finalDamage *= playerStats.GetValue(
        StatType.DamageTakenMultiplier
    );

    if (finalDamage <= 0f)
        return;

    bool aboveThreshold =
        currentHealth >=
        maxHealth * healthGateThreshold;

    bool wouldDie =
        finalDamage >= currentHealth;

    bool canTriggerHealthGate =
        healthGateAvailable &&
        aboveThreshold &&
        wouldDie &&
        !damageInfo.IsSecondary;

    if (canTriggerHealthGate)
    {
        float damageTaken = Mathf.Max(0f, currentHealth - 1f);
        currentHealth = 1f;
        healthGateAvailable = false;

        StartCoroutine(InvulnerabilityRoutine());

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        OnDamaged?.Invoke(finalDamage);
        GetCombatEvents()?.ReportDamageTaken(damageTaken);
        OnHealthGateTriggered?.Invoke();

        return;
    }

    float healthBeforeDamage = currentHealth;

    currentHealth = Mathf.Max(
        currentHealth - finalDamage,
        0f
    );

    OnHealthChanged?.Invoke(
        currentHealth,
        maxHealth
    );

    OnDamaged?.Invoke(finalDamage);
    GetCombatEvents()?.ReportDamageTaken(
        healthBeforeDamage - currentHealth
    );

    if (IsDead)
        Die();
}
private IEnumerator InvulnerabilityRoutine()
{
    isInvulnerable = true;

    yield return new WaitForSeconds(
        invulnerabilityDuration
    );

    isInvulnerable = false;
}
private float CalculateDamage(DamagePart part)
{
    float kineticDefense =
        playerStats.GetValue(StatType.Armor);

    float spiritualDefense =
        playerStats.GetValue(
            StatType.SpiritualDefense
        );

    return part.damageType switch
    {
        DamageType.Kinetic =>
            ApplyDefense(part.damage, kineticDefense),

        DamageType.Spiritual or
        DamageType.Fire or
        DamageType.Lightning or
        DamageType.Frost or
        DamageType.Decay =>
            ApplyDefense(part.damage, spiritualDefense),

        DamageType.Holy => part.damage,
        DamageType.Cursed => part.damage,

        _ => part.damage
    };
}

private float ApplyDefense(
    float damage,
    float defense)
{
    defense = Mathf.Max(0f, defense);

    return damage * (100f / (100f + defense));
}

private PlayerCombatEvents GetCombatEvents()
{
    if (combatEvents == null)
        combatEvents = GetComponent<PlayerCombatEvents>();

    return combatEvents;
}


    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead)
            return;

        float healingPower =
            playerStats.GetValue(StatType.HealingPower);

        float finalHealing = amount * healingPower;

        currentHealth = Mathf.Min(
            currentHealth + finalHealing,
            maxHealth
        );
        
        if (currentHealth >=
    maxHealth * healthGateThreshold)
{
    healthGateAvailable = true;
}

OnHealthChanged?.Invoke(
    currentHealth,
    maxHealth
);
    }

    private void Die()
    {
        Debug.Log("Игрок умер");
    }
}

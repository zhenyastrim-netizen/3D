using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerStats playerStats;

    private float currentHealth;
    private float maxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        playerStats.OnStatChanged += HandleStatChanged;
    }

    private void Start()
    {
        maxHealth = playerStats.GetValue(StatType.MaxHealth);
        currentHealth = maxHealth;
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
    }

    public void TakeDamage(DamageInfo damageInfo)
{
    if (IsDead || damageInfo.Parts == null)
        return;

    float finalDamage = 0f;

    foreach (DamagePart part in damageInfo.Parts)
    {
        finalDamage += CalculateDamage(part);
    }

    if (finalDamage <= 0f)
        return;

    currentHealth = Mathf.Max(
        currentHealth - finalDamage,
        0f
    );

    Debug.Log(
        $"Игрок получил {finalDamage:F1} урона. " +
        $"HP: {currentHealth:F1}/{maxHealth:F1}"
    );

    if (IsDead)
        Die();
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
private float CalculateIncomingDamage(float damage)
{
    float armor = Mathf.Max(
        0f,
        playerStats.GetValue(StatType.Armor)
    );

    return damage * (100f / (100f + armor));
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
    }

    private void Die()
    {
        Debug.Log("Игрок умер");
    }
}
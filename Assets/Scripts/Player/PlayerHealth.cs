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
    if (damageInfo.Amount <= 0f || IsDead)
        return;

    float finalDamage =
        CalculateIncomingDamage(damageInfo.Amount);

    currentHealth = Mathf.Max(
        currentHealth - finalDamage,
        0f
    );

    Debug.Log(
        $"Урон: {finalDamage:F1} | " +
        $"Тип: {damageInfo.Type} | " +
        $"Крит: {damageInfo.IsCritical} | " +
        $"HP: {currentHealth:F1}/{maxHealth:F1}"
    );

    if (IsDead)
        Die();
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
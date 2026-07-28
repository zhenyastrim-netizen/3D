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

    public void TakeDamage(float damage)
{
    if (damage <= 0f || IsDead)
        return;

    float armor = playerStats.GetValue(StatType.Armor);
    float damageReduction = armor / (armor + 100f);
    float finalDamage = damage * (1f - damageReduction);

    currentHealth = Mathf.Max(
        currentHealth - finalDamage,
        0f
    );

    Debug.Log(
        $"Получено урона: {finalDamage:F1} " +
        $"(исходный: {damage:F1}, броня: {armor:F1}). " +
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
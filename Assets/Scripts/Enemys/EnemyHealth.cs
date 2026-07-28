using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Damage multipliers")]
    [SerializeField, Min(0f)]
    private float rangedDamageMultiplier = 1f;

    [SerializeField, Min(0f)]
    private float meleeDamageMultiplier = 1f;

    [SerializeField, Min(0f)]
    private float magicDamageMultiplier = 1f;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0f;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead || damageInfo.Amount <= 0f)
            return;

        float multiplier =
            GetDamageMultiplier(damageInfo.Type);

        float finalDamage =
            damageInfo.Amount * multiplier;

        currentHealth = Mathf.Max(
            currentHealth - finalDamage,
            0f
        );

        Debug.Log(
            $"{gameObject.name} получил {finalDamage:F1} урона. " +
            $"Тип: {damageInfo.Type}. " +
            $"Крит: {damageInfo.IsCritical}. " +
            $"HP: {currentHealth:F1}/{maxHealth:F1}"
        );

        if (currentHealth <= 0f)
            Die();
    }

    private float GetDamageMultiplier(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Ranged => rangedDamageMultiplier,
            DamageType.Melee => meleeDamageMultiplier,
            DamageType.Magic => magicDamageMultiplier,
            _ => 1f
        };
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

        Debug.Log($"{gameObject.name} умер");

        EnemyBrain brain = GetComponent<EnemyBrain>();
        brain?.SetDead();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}
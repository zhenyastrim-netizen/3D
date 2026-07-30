using UnityEngine;

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
    if (isDead || damageInfo.Parts == null)
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

        DamageType.Holy =>
            part.damage,

        DamageType.Cursed =>
            part.damage,

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
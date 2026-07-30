using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
{
    if (damageInfo.TotalDamage <= 0f)
        return;

    currentHealth -= damageInfo.TotalDamage;
    currentHealth = Mathf.Max(currentHealth, 0f);

    Debug.Log(
        $"Цель получила {damageInfo.TotalDamage:F1} урона. " +
        $"Тип: {damageInfo.TotalDamage}. " +
        $"Крит: {damageInfo.IsCritical}"
    );

    if (currentHealth <= 0f)
        Die();
}

    private void Die()
    {
        Destroy(gameObject);
    }
}
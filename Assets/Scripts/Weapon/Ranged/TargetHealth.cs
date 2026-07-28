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
    if (damageInfo.Amount <= 0f)
        return;

    currentHealth -= damageInfo.Amount;
    currentHealth = Mathf.Max(currentHealth, 0f);

    Debug.Log(
        $"Цель получила {damageInfo.Amount:F1} урона. " +
        $"Тип: {damageInfo.Type}. " +
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
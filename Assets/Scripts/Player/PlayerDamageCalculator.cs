using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerDamageCalculator : MonoBehaviour
{
    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public DamageInfo CreateDamage(
        float baseDamage,
        DamageType damageType,
        GameObject source)
    {
        float damageMultiplier =
            GetDamageMultiplier(damageType);

        float finalDamage =
            baseDamage * damageMultiplier;

        float criticalChance = Mathf.Clamp01(
            playerStats.GetValue(StatType.CriticalChance)
        );

        bool isCritical =
            Random.value < criticalChance;

        if (isCritical)
        {
            float criticalDamage = playerStats.GetValue(
                StatType.CriticalDamage
            );

            finalDamage *= criticalDamage;
        }

        return new DamageInfo(
            finalDamage,
            damageType,
            isCritical,
            source
        );
    }

    private float GetDamageMultiplier(
        DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Ranged =>
                playerStats.GetValue(StatType.RangedDamage),

            DamageType.Melee =>
                playerStats.GetValue(StatType.MeleeDamage),

            DamageType.Magic =>
                playerStats.GetValue(StatType.MagicDamage),

            _ => 1f
        };
    }
}
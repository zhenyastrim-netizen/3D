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
        DamagePart[] baseParts,
        AttackType attackType,
        GameObject source)
    {
        if (baseParts == null)
            baseParts = new DamagePart[0];

        float attackMultiplier =
            GetAttackMultiplier(attackType);

        float criticalChance = Mathf.Clamp01(
            playerStats.GetValue(StatType.CriticalChance)
        );

        bool isCritical =
            Random.value < criticalChance;

        float criticalMultiplier = isCritical
            ? playerStats.GetValue(StatType.CriticalDamage)
            : 1f;

        DamagePart[] finalParts =
            new DamagePart[baseParts.Length];

        for (int i = 0; i < baseParts.Length; i++)
        {
            DamagePart part = baseParts[i];

            part.damage *=
                attackMultiplier *
                criticalMultiplier;

            finalParts[i] = part;
        }

        return new DamageInfo(
            finalParts,
            attackType,
            isCritical,
            source
        );
    }

    private float GetAttackMultiplier(
        AttackType attackType)
    {
        return attackType switch
        {
            AttackType.Ranged =>
                playerStats.GetValue(StatType.RangedDamage),

            AttackType.Melee =>
                playerStats.GetValue(StatType.MeleeDamage),

            AttackType.Magic =>
                playerStats.GetValue(StatType.MagicDamage),

            _ => 1f
        };
    }
}
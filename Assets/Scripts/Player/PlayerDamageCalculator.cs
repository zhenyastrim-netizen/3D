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
            playerStats.GetValue(
                StatType.CriticalChance
            )
        );

        bool isCritical =
            Random.value < criticalChance;

        float criticalMultiplier = isCritical
            ? playerStats.GetValue(
                StatType.CriticalDamage
            )
            : 1f;

        DamagePart[] finalParts =
            new DamagePart[baseParts.Length];

        for (int i = 0; i < baseParts.Length; i++)
        {
            DamagePart part = baseParts[i];

            float typeMultiplier =
                GetDamageTypeMultiplier(
                    part.damageType
                );

            part.damage *=
                attackMultiplier *
                typeMultiplier *
                criticalMultiplier;

            part.buildup *= typeMultiplier;

            finalParts[i] = part;
        }

        return new DamageInfo(
            finalParts,
            attackType,
            isCritical,
            source
        );
    }

    private float GetDamageTypeMultiplier(
        DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Spiritual:
            case DamageType.Fire:
            case DamageType.Lightning:
            case DamageType.Frost:
            case DamageType.Decay:
                return playerStats.GetValue(
                    StatType.SpiritPower
                );

            default:
                return 1f;
        }
    }

    private float GetAttackMultiplier(
        AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Ranged:
                return playerStats.GetValue(
                    StatType.RangedDamage
                );

            case AttackType.Melee:
                return playerStats.GetValue(
                    StatType.MeleeDamage
                );

            case AttackType.Magic:
                return playerStats.GetValue(
                    StatType.MagicDamage
                );

            default:
                return 1f;
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerDamageCalculator : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerHumanity playerHumanity;
    private ElementalSkillRuntime elementalSkills;
    private MagicSkillRuntime magicSkills;
    private ShootingSkillRuntime shootingSkills;

    private void Awake()
    {
        playerHumanity =
    GetComponent<PlayerHumanity>();
        playerStats = GetComponent<PlayerStats>();
        elementalSkills = GetComponent<ElementalSkillRuntime>();
        magicSkills = GetComponent<MagicSkillRuntime>();
        shootingSkills = GetComponent<ShootingSkillRuntime>();
    }

    public DamageInfo CreateDamage(
        DamagePart[] baseParts,
        AttackType attackType,
        GameObject source,
        bool isSecondary = false)
    {
        if (baseParts == null)
            baseParts = new DamagePart[0];

        baseParts = AddElementalBulletPart(
            baseParts,
            attackType,
            isSecondary
        );

        float attackMultiplier =
            GetAttackMultiplier(attackType);

        float criticalChance = Mathf.Clamp01(
            playerStats.GetValue(
                StatType.CriticalChance
            )
        );

        bool canCrit = attackType != AttackType.Magic ||
                       GetMagicSkills()?.CanMagicCrit == true;

        bool isCritical =
            canCrit && Random.value < criticalChance;

        float criticalMultiplier = isCritical
            ? playerStats.GetValue(
                StatType.CriticalDamage
            )
            : 1f;

        DamagePart[] finalParts =
            new DamagePart[baseParts.Length];

        float shootingHitMultiplier =
            GetShootingHitMultiplier(
                attackType,
                isSecondary
            );

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
                criticalMultiplier *
                shootingHitMultiplier;

            part.buildup *= typeMultiplier;

            finalParts[i] = part;
        }

        return new DamageInfo(
            finalParts,
            attackType,
            isCritical,
            source,
            isSecondary
        );
    }

    private MagicSkillRuntime GetMagicSkills()
    {
        if (magicSkills == null)
            magicSkills = GetComponent<MagicSkillRuntime>();

        return magicSkills;
    }

    private float GetShootingHitMultiplier(
        AttackType attackType,
        bool isSecondary)
    {
        if (attackType != AttackType.Ranged || isSecondary)
            return 1f;

        if (shootingSkills == null)
            shootingSkills = GetComponent<ShootingSkillRuntime>();

        return shootingSkills != null
            ? shootingSkills.ConsumeHitMultiplier()
            : 1f;
    }

    private float GetDamageTypeMultiplier(
        DamageType damageType)
    {
        if (elementalSkills == null)
            elementalSkills = GetComponent<ElementalSkillRuntime>();

        switch (damageType)
        {
            case DamageType.Holy:
    return playerHumanity != null
        ? playerHumanity.GetHolyDamageMultiplier()
        : 1f;

case DamageType.Cursed:
    return playerHumanity != null
        ? playerHumanity.GetCursedDamageMultiplier()
        : 1f;
            case DamageType.Spiritual:
            case DamageType.Fire:
            case DamageType.Lightning:
            case DamageType.Frost:
            case DamageType.Decay:
                float spiritMultiplier = playerStats.GetValue(
                    StatType.SpiritPower
                );

                float elementalMultiplier = elementalSkills != null
                    ? elementalSkills.GetDamageMultiplier(damageType)
                    : 1f;

                return spiritMultiplier * elementalMultiplier;

            default:
                return 1f;
        }
    }

    private DamagePart[] AddElementalBulletPart(
        DamagePart[] baseParts,
        AttackType attackType,
        bool isSecondary)
    {
        if (attackType != AttackType.Ranged || isSecondary)
            return baseParts;

        if (elementalSkills == null)
            elementalSkills = GetComponent<ElementalSkillRuntime>();

        if (elementalSkills == null ||
            !elementalSkills.TryCreateElementalBulletPart(
                baseParts,
                out DamagePart elementalPart))
        {
            return baseParts;
        }

        DamagePart[] result =
            new DamagePart[baseParts.Length + 1];

        for (int i = 0; i < baseParts.Length; i++)
            result[i] = baseParts[i];

        result[result.Length - 1] = elementalPart;
        return result;
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

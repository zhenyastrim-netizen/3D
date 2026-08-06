using UnityEngine;

[DisallowMultipleComponent]
public class ElementalSkillRuntime : MonoBehaviour
{
    private int elementalBulletsRank;
    private float elementalBulletChancePerRank;
    private float elementalBulletDamageMultiplier;
    private float elementalBulletBuildup;
    private float elementalStatusDurationPerRank;

    private int fireGreetingRank;
    private float fireDamagePerRank;
    private float fireDurationPerRank;

    private int coldWinterRank;
    private float frostDamagePerRank;
    private float frostArmorReductionPerRank;

    private int stormWarningRank;
    private float lightningDamagePerRank;
    private int lightningJumpsPerRank;
    private float temporaryStatusDurationBonus;

    public void ConfigureElementalBullets(
        int rank,
        float chancePerRank,
        float damageMultiplier,
        float buildup,
        float statusDurationPerRank)
    {
        elementalBulletsRank = Mathf.Max(1, rank);
        elementalBulletChancePerRank = Mathf.Clamp01(chancePerRank);
        elementalBulletDamageMultiplier = Mathf.Max(0f, damageMultiplier);
        elementalBulletBuildup = Mathf.Max(0f, buildup);
        elementalStatusDurationPerRank = Mathf.Max(0f, statusDurationPerRank);
    }

    public void ConfigureFireGreeting(
        int rank,
        float damagePerRank,
        float durationPerRank)
    {
        fireGreetingRank = Mathf.Max(1, rank);
        fireDamagePerRank = Mathf.Max(0f, damagePerRank);
        fireDurationPerRank = Mathf.Max(0f, durationPerRank);
    }

    public void ConfigureColdWinter(
        int rank,
        float damagePerRank,
        float armorReductionPerRank)
    {
        coldWinterRank = Mathf.Max(1, rank);
        frostDamagePerRank = Mathf.Max(0f, damagePerRank);
        frostArmorReductionPerRank = Mathf.Max(0f, armorReductionPerRank);
    }

    public void ConfigureStormWarning(
        int rank,
        float damagePerRank,
        int jumpsPerRank)
    {
        stormWarningRank = Mathf.Max(1, rank);
        lightningDamagePerRank = Mathf.Max(0f, damagePerRank);
        lightningJumpsPerRank = Mathf.Max(0, jumpsPerRank);
    }

    public float GetDamageMultiplier(DamageType damageType)
    {
        float bonus = damageType switch
        {
            DamageType.Fire => fireGreetingRank * fireDamagePerRank,
            DamageType.Frost => coldWinterRank * frostDamagePerRank,
            DamageType.Lightning => stormWarningRank * lightningDamagePerRank,
            _ => 0f
        };

        return 1f + bonus;
    }

    public float GetStatusDurationMultiplier()
    {
        return 1f +
               elementalBulletsRank *
               elementalStatusDurationPerRank +
               temporaryStatusDurationBonus;
    }

    public void SetTemporaryStatusDurationBonus(float bonus)
    {
        temporaryStatusDurationBonus = Mathf.Max(0f, bonus);
    }

    public float GetFireDurationMultiplier()
    {
        return GetStatusDurationMultiplier() +
               fireGreetingRank * fireDurationPerRank;
    }

    public float GetFrostArmorReductionBonus()
    {
        return coldWinterRank * frostArmorReductionPerRank;
    }

    public int GetAdditionalLightningJumps()
    {
        return stormWarningRank * lightningJumpsPerRank;
    }

    public bool TryCreateElementalBulletPart(
        DamagePart[] baseParts,
        out DamagePart elementalPart)
    {
        elementalPart = default;

        if (elementalBulletsRank <= 0 ||
            baseParts == null ||
            baseParts.Length == 0)
        {
            return false;
        }

        float chance = Mathf.Clamp01(
            elementalBulletsRank *
            elementalBulletChancePerRank
        );

        if (Random.value >= chance)
            return false;

        float baseDamage = 0f;

        foreach (DamagePart part in baseParts)
            baseDamage += Mathf.Max(0f, part.damage);

        if (baseDamage <= 0f)
            return false;

        DamageType[] elements =
        {
            DamageType.Fire,
            DamageType.Lightning,
            DamageType.Frost,
            DamageType.Decay
        };

        DamageType selectedElement = elements[
            Random.Range(0, elements.Length)
        ];

        elementalPart = new DamagePart(
            selectedElement,
            baseDamage * elementalBulletDamageMultiplier,
            elementalBulletBuildup
        );

        return true;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class WeaponLootGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponAffixPool affixPool;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerHumanity playerHumanity;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats =
                FindFirstObjectByType<PlayerStats>();
        }

        if (playerHumanity == null)
        {
            playerHumanity =
                FindFirstObjectByType<PlayerHumanity>();
        }
    }

    public WeaponInstance GenerateRandom(
        WeaponData weaponData)
    {
        ItemRarity rarity = RollRarity();
        ItemAlignment alignment = RollAlignment();

        return Generate(
            weaponData,
            rarity,
            alignment
        );
    }

    public WeaponInstance GenerateUnique(
        WeaponData weaponData)
    {
        return Generate(
            weaponData,
            ItemRarity.Unique,
            RollAlignment()
        );
    }

    private WeaponInstance Generate(
        WeaponData weaponData,
        ItemRarity rarity,
        ItemAlignment alignment)
    {
        WeaponInstance instance = new WeaponInstance(
            weaponData,
            rarity,
            alignment
        );

        HashSet<WeaponAffixDefinition> used =
            new HashSet<WeaponAffixDefinition>();

        int normalAffixCount =
            GetRarityAffixCount(rarity);

        AddAffixes(
            instance,
            normalAffixCount,
            false,
            used
        );

        switch (alignment)
        {
            case ItemAlignment.Sanctified:
                AddAffixes(
                    instance,
                    Random.Range(1, 6),
                    false,
                    used
                );
                break;

            case ItemAlignment.Cursed:
                AddAffixes(
                    instance,
                    Random.Range(1, 4),
                    false,
                    used
                );

                AddAffixes(
                    instance,
                    Random.Range(1, 4),
                    true,
                    used
                );
                break;
        }

        return instance;
    }

    private void AddAffixes(
        WeaponInstance instance,
        int count,
        bool negative,
        HashSet<WeaponAffixDefinition> used)
    {
        for (int i = 0; i < count; i++)
        {
            WeaponAffixDefinition definition =
                affixPool.Roll(
                    instance.BaseData,
                    negative,
                    used
                );

            if (definition == null)
                break;

            WeaponAffix affix =
                definition.Roll(negative);

            instance.AddAffix(affix);
            used.Add(definition);
        }
    }

    private int GetRarityAffixCount(
        ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 0;

            case ItemRarity.Rare:
                return Random.Range(1, 3);

            case ItemRarity.Legendary:
                return Random.Range(3, 6);

            case ItemRarity.Unique:
                return Random.Range(1, 6);

            default:
                return 0;
        }
    }

    private ItemRarity RollRarity()
    {
        float luck = playerStats != null
            ? playerStats.GetValue(StatType.Luck)
            : 0f;

        float normalizedLuck =
            luck / (luck + 100f);

        float legendaryChance = Mathf.Lerp(
            0.03f,
            0.15f,
            normalizedLuck
        );

        float rareChance = Mathf.Lerp(
            0.22f,
            0.45f,
            normalizedLuck
        );

        float roll = Random.value;

        if (roll < legendaryChance)
            return ItemRarity.Legendary;

        if (roll < legendaryChance + rareChance)
            return ItemRarity.Rare;

        return ItemRarity.Common;
    }

    private ItemAlignment RollAlignment()
    {
        if (playerHumanity == null)
            return ItemAlignment.Neutral;

        float value = playerHumanity.CurrentValue;

        if (Mathf.Approximately(value, 0f))
            return ItemAlignment.Neutral;

        float strength =
            Mathf.Abs(value) / 100f;

        float alignedChance = Mathf.Lerp(
            0.02f,
            0.20f,
            strength
        );

        if (Random.value > alignedChance)
            return ItemAlignment.Neutral;

        return value > 0f
            ? ItemAlignment.Sanctified
            : ItemAlignment.Cursed;
    }
}
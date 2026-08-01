using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsUI : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject emptyState;

    [Header("Information")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemTypeText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text alignmentText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;

    private void OnEnable()
    {
        InventorySlotUI.OnItemHovered += ShowItem;
        ShowItem(null);
    }

    private void OnDisable()
    {
        InventorySlotUI.OnItemHovered -= ShowItem;
    }

    private void ShowItem(InventoryItem inventoryItem)
    {
        ItemData item = inventoryItem?.Item;
        bool hasItem = item != null;

        if (contentRoot != null)
            contentRoot.SetActive(hasItem);

        if (emptyState != null)
            emptyState.SetActive(!hasItem);

        if (!hasItem)
            return;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (itemTypeText != null)
            itemTypeText.text = GetItemTypeText(item.itemType);

        if (descriptionText != null)
            descriptionText.text = item.description;

        WeaponInstance weapon =
            inventoryItem.WeaponInstance;

        bool isWeapon = weapon != null;

        if (rarityText != null)
            rarityText.gameObject.SetActive(isWeapon);

        if (alignmentText != null)
            alignmentText.gameObject.SetActive(isWeapon);

        if (isWeapon)
        {
            if (rarityText != null)
            {
                rarityText.text =
                    GetRarityText(weapon.Rarity);
            }

            if (alignmentText != null)
            {
                alignmentText.text =
                    GetAlignmentText(weapon.Alignment);
            }

            if (statsText != null)
                statsText.text = BuildWeaponStats(weapon);
        }
        else
        {
            if (statsText != null)
                statsText.text = BuildItemStats(item);
        }
    }

    private string BuildWeaponStats(
        WeaponInstance weapon)
    {
        if (weapon.Affixes.Count == 0)
            return "Нет дополнительных свойств";

        StringBuilder builder = new StringBuilder();

        foreach (WeaponAffix affix in weapon.Affixes)
        {
            float value = affix.Value;

            string valueText =
                affix.ModifierType ==
                StatModifierType.Percent
                    ? $"{value * 100f:+0.#;-0.#}%"
                    : $"{value:+0.##;-0.##}";

            builder.AppendLine(
                $"{affix.Definition.AffixName}: " +
                $"{valueText}"
            );
        }

        return builder.ToString();
    }

    private string BuildItemStats(ItemData item)
    {
        if (item.effects == null ||
            item.effects.Length == 0)
        {
            return "Нет дополнительных свойств";
        }

        StringBuilder builder = new StringBuilder();

        bool hasStats = false;

        foreach (StatEffect effect in item.effects)
        {
            if (effect == null)
                continue;

            if (!string.IsNullOrWhiteSpace(effect.EffectName))
                builder.AppendLine($"<b>{effect.EffectName}</b>");

            if (effect.Modifications == null)
                continue;

            foreach (StatEffectEntry entry in effect.Modifications)
            {
                if (entry == null)
                    continue;

                builder.AppendLine(
                    $"{GetStatName(entry.statType)}: " +
                    FormatModifier(entry)
                );

                hasStats = true;
            }
        }

        return hasStats
            ? builder.ToString().TrimEnd()
            : "Нет дополнительных свойств";
    }

    private string FormatModifier(StatEffectEntry entry)
    {
        if (entry.modifierType == StatModifierType.Percent)
            return $"{entry.value * 100f:+0.#;-0.#;0}%";

        if (entry.statType == StatType.CriticalChance)
            return $"{entry.value * 100f:+0.#;-0.#;0} п.п.";

        return $"{entry.value:+0.##;-0.##;0}";
    }

    private string GetItemTypeText(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                return "Оружие";
            case ItemType.Consumable:
                return "Расходуемый предмет";
            case ItemType.HealingFlask:
                return "Лечебная фляга";
            case ItemType.Passive:
                return "Пассивный предмет";
            case ItemType.PathEffect:
                return "Эффект пути";
            case ItemType.Quest:
                return "Квестовый предмет";
            default:
                return itemType.ToString();
        }
    }

    private string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                return "Максимальное здоровье";
            case StatType.MoveSpeed:
                return "Скорость движения";
            case StatType.MeleeDamage:
                return "Урон ближнего боя";
            case StatType.RangedDamage:
                return "Урон оружия";
            case StatType.MagicDamage:
                return "Магический урон";
            case StatType.AttackSpeed:
                return "Скорость атаки";
            case StatType.ReloadSpeed:
                return "Скорость перезарядки";
            case StatType.MagazineSize:
                return "Размер магазина";
            case StatType.CriticalChance:
                return "Шанс критического удара";
            case StatType.CriticalDamage:
                return "Критический урон";
            case StatType.Armor:
                return "Броня";
            case StatType.Luck:
                return "Удача";
            case StatType.HealingPower:
                return "Сила лечения";
            case StatType.SpiritualDefense:
                return "Духовная защита";
            case StatType.SpiritPower:
                return "Сила духа";
            default:
                return statType.ToString();
        }
    }

    private string GetRarityText(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare:
                return "<color=#4D9EFF>Редкое</color>";

            case ItemRarity.Legendary:
                return "<color=#FF9D32>Легендарное</color>";

            case ItemRarity.Unique:
                return "<color=#D45CFF>Уникальное</color>";

            default:
                return "<color=#B8B8B8>Обычное</color>";
        }
    }

    private string GetAlignmentText(
        ItemAlignment alignment)
    {
        switch (alignment)
        {
            case ItemAlignment.Sanctified:
                return "<color=#FFE58A>Освящённое</color>";

            case ItemAlignment.Cursed:
                return "<color=#B866FF>Проклятое</color>";

            default:
                return "Нейтральное";
        }
    }
}

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

        contentRoot.SetActive(hasItem);
        emptyState.SetActive(!hasItem);

        if (!hasItem)
            return;

        icon.sprite = item.icon;
        icon.enabled = item.icon != null;

        itemNameText.text = item.itemName;
        itemTypeText.text = item.itemType.ToString();
        descriptionText.text = item.description;

        WeaponInstance weapon =
            inventoryItem.WeaponInstance;

        bool isWeapon = weapon != null;

        rarityText.gameObject.SetActive(isWeapon);
        alignmentText.gameObject.SetActive(isWeapon);

        if (isWeapon)
        {
            rarityText.text =
                GetRarityText(weapon.Rarity);

            alignmentText.text =
                GetAlignmentText(weapon.Alignment);

            statsText.text =
                BuildWeaponStats(weapon);
        }
        else
        {
            statsText.text =
                BuildItemStats(item);
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

        foreach (StatEffect effect in item.effects)
        {
            if (effect == null)
                continue;

            builder.AppendLine(effect.EffectName);
        }

        return builder.ToString();
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
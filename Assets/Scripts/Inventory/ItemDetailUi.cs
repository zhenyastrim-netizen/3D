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

    [Header("Rarity Background")]
    [SerializeField] private Image cardBackground;

    [SerializeField] private Color commonBackground =
        new Color32(23, 25, 31, 245);

    [SerializeField] private Color rareBackground =
        new Color32(18, 35, 58, 245);

    [SerializeField] private Color legendaryBackground =
        new Color32(56, 35, 15, 245);

    [SerializeField] private Color uniqueBackground =
        new Color32(42, 21, 56, 245);

    [Header("Layout")]
    [SerializeField] private bool configureLayoutAutomatically = true;
    [SerializeField] private Vector2 cardSize = new Vector2(680f, 580f);
    [SerializeField, Min(0f)] private float cardPadding = 24f;
    [SerializeField] private bool showItemIcon = false;

    private void Awake()
    {
        Image assignedBackground = cardBackground;
        Image ownBackground = GetComponent<Image>();

        // Фон всегда находится на корневом объекте карточки и поэтому
        // автоматически растягивается вместе с ее RectTransform.
        

        // Старые сцены могли ссылаться на отдельный квадрат вместо фона.
        if (assignedBackground != null &&
            assignedBackground != ownBackground &&
            !assignedBackground.transform.IsChildOf(transform))
        {
            assignedBackground.gameObject.SetActive(false);
        }

        cardBackground = ownBackground;

        if (configureLayoutAutomatically)
            ConfigureLayout();

        if (cardBackground != null)
        {
            cardBackground.enabled = true;
            cardBackground.raycastTarget = false;
            SetRarityBackground(ItemRarity.Common);
        }
    }
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

            bool shouldShowIcon =
                showItemIcon && item.icon != null;

            icon.enabled = shouldShowIcon;
            icon.gameObject.SetActive(shouldShowIcon);
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

        SetRarityBackground(
            isWeapon
                ? weapon.Rarity
                : ItemRarity.Common
        );

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
    private void SetRarityBackground(
        ItemRarity rarity)
    {
        if (cardBackground == null)
            return;

        Color rarityColor;

        switch (rarity)
        {
            case ItemRarity.Rare:
                cardBackground.color = rareBackground;
                rarityColor = new Color32(73, 165, 255, 255);
                break;

            case ItemRarity.Legendary:
                cardBackground.color = legendaryBackground;
                rarityColor = new Color32(255, 166, 61, 255);
                break;

            case ItemRarity.Unique:
                cardBackground.color = uniqueBackground;
                rarityColor = new Color32(202, 112, 255, 255);
                break;

            default:
                cardBackground.color = commonBackground;
                rarityColor = new Color32(190, 196, 208, 255);
                break;
        }

        if (rarityText != null)
            rarityText.color = rarityColor;
    }

    private void ConfigureLayout()
    {
        RectTransform cardRect = transform as RectTransform;

        if (cardRect != null)
        {
            cardRect.localScale = Vector3.one;
            cardRect.sizeDelta = cardSize;
        }

        RectTransform contentRect = contentRoot != null
            ? contentRoot.transform as RectTransform
            : null;

        if (contentRect != null)
        {
            VerticalLayoutGroup oldLayout =
                contentRoot.GetComponent<VerticalLayoutGroup>();

            ContentSizeFitter oldFitter =
                contentRoot.GetComponent<ContentSizeFitter>();

            if (oldLayout != null)
                oldLayout.enabled = false;

            if (oldFitter != null)
                oldFitter.enabled = false;

            contentRect.localScale = Vector3.one;
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.offsetMin = new Vector2(cardPadding, cardPadding);
            contentRect.offsetMax = new Vector2(-cardPadding, -cardPadding);
        }

        RectTransform emptyRect = emptyState != null
            ? emptyState.transform as RectTransform
            : null;

        if (emptyRect != null)
        {
            emptyRect.localScale = Vector3.one;
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.offsetMin = Vector2.zero;
            emptyRect.offsetMax = Vector2.zero;
        }

        ConfigureIcon(icon);

        if (icon != null)
            icon.gameObject.SetActive(showItemIcon && icon.sprite != null);

        float iconSpace = showItemIcon ? 112f : 0f;

        SetTopStretch(itemNameText, 0f, 40f, 0f, iconSpace);
        SetTopStretch(itemTypeText, 46f, 26f, 0f, iconSpace);
        SetTopStretch(descriptionText, 92f, 70f, 0f, 0f);
        SetVerticalStretch(statsText, 178f, 72f, 0f, 0f);

        SetBottomLeft(rarityText, 30f, 210f, 25f);
        SetBottomLeft(alignmentText, 0f, 210f, 25f);

        ConfigureText(
            itemNameText,
            28f,
            new Color32(242, 244, 248, 255),
            TextAlignmentOptions.TopLeft
        );

        ConfigureText(
            itemTypeText,
            16f,
            new Color32(145, 153, 168, 255),
            TextAlignmentOptions.TopLeft
        );

        ConfigureText(
            descriptionText,
            17f,
            new Color32(203, 208, 218, 255),
            TextAlignmentOptions.TopLeft
        );

        ConfigureText(
            statsText,
            16f,
            new Color32(225, 228, 235, 255),
            TextAlignmentOptions.TopLeft
        );

        if (statsText != null)
        {
            statsText.enableAutoSizing = true;
            statsText.fontSizeMin = 11f;
            statsText.fontSizeMax = 16f;
            statsText.overflowMode = TextOverflowModes.Overflow;
        }

        ConfigureText(
            rarityText,
            17f,
            Color.white,
            TextAlignmentOptions.BottomLeft
        );

        ConfigureText(
            alignmentText,
            17f,
            new Color32(173, 179, 190, 255),
            TextAlignmentOptions.BottomLeft
        );
    }

    private static void ConfigureIcon(Image targetIcon)
    {
        if (targetIcon == null)
            return;

        RectTransform rect = targetIcon.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(88f, 88f);

        targetIcon.preserveAspect = true;
        targetIcon.color = Color.white;
        targetIcon.raycastTarget = false;
    }

    private static void ConfigureText(
        TMP_Text text,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        text.rectTransform.localScale = Vector3.one;
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }

    private static void SetTopStretch(
        TMP_Text text,
        float top,
        float height,
        float left,
        float right)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -top - height);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void SetBottomLeft(
        TMP_Text text,
        float bottom,
        float width,
        float height)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(0f, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetVerticalStretch(
        TMP_Text text,
        float top,
        float bottom,
        float left,
        float right)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private string BuildWeaponStats(
    WeaponInstance weapon)
{
    if (weapon == null || weapon.BaseData == null)
        return "";

    WeaponData data = weapon.BaseData;
    StringBuilder builder = new StringBuilder();

    switch (data.WeaponType)
    {
        case WeaponType.Ranged:
            builder.AppendLine(
                $"Урон: {data.Damage:0.##}"
            );

            builder.AppendLine(
                $"Скорострельность: {data.FireRate:0.##}/с"
            );

            builder.AppendLine(
                $"Магазин: {data.MagazineSize}"
            );

            builder.AppendLine(
                $"Перезарядка: {data.ReloadTime:0.##} сек."
            );
            break;

        case WeaponType.Melee:
            builder.AppendLine(
                $"Урон: {data.Damage:0.##}"
            );

            builder.AppendLine(
                $"Скорость атаки: " +
                $"{data.MeleeAttacksPerSecond:0.##}/с"
            );

            builder.AppendLine(
                $"Дальность: {data.MeleeRange:0.##}"
            );
            break;

        case WeaponType.Magic:
            builder.AppendLine("Магическое оружие");
            break;
    }

    if (weapon.Affixes.Count > 0)
    {
        builder.AppendLine();
        builder.AppendLine(
            "<b>Дополнительные свойства</b>"
        );

        foreach (WeaponAffix affix in weapon.Affixes)
        {
            float value = affix.Value;

            string valueText =
                affix.ModifierType ==
                StatModifierType.Percent
                    ? $"{value * 100f:+0.#;-0.#}%"
                    : $"{value:+0.##;-0.##}";

            string statName =
    string.IsNullOrWhiteSpace(
        affix.Definition.AffixName
    )
        ? GetStatName(affix.StatType)
        : affix.Definition.AffixName;

builder.AppendLine(
    $"{statName}: {valueText}"
);
        }
    }
    else
    {
        builder.AppendLine();
        builder.AppendLine(
            "<color=#78808F>Нет дополнительных свойств</color>"
        );
    }

    return builder.ToString().TrimEnd();
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

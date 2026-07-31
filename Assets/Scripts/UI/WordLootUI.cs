using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldLootCardUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pickupText;
[Header("World tracking")]
[SerializeField] private RectTransform panelRect;
[SerializeField] private Vector3 worldOffset =
    Vector3.up * 1.2f;
    [Header("Card visual")]
[SerializeField] private Image rarityFrame;
[SerializeField] private Image background;
[SerializeField] private TMP_Text alignmentText;

[SerializeField] private Vector2 screenOffset =
    new Vector2(40f, 20f);

private Transform trackedTarget;
private Camera playerCamera;
    private void Awake()
{
    playerCamera = Camera.main;
    Hide();
}

    public void ShowWeapon(WeaponInstance instance)
    {
        if (instance == null ||
            instance.BaseData == null)
        {
            Hide();
            return;
        }

        WeaponData weapon = instance.BaseData;

        panel.SetActive(true);

        icon.sprite = weapon.icon;
        icon.enabled = weapon.icon != null;

        nameText.text = weapon.itemName;

        rarityText.text =
            $"{GetRarityName(instance.Rarity)} " +
            $"{GetAlignmentName(instance.Alignment)}";

        rarityText.color =
            GetRarityColor(instance.Rarity);

        StringBuilder stats = new StringBuilder();

        stats.AppendLine($"Урон: {weapon.Damage:F1}");
        stats.AppendLine($"Скорострельность: {weapon.FireRate:F1}");
        stats.AppendLine($"Магазин: {weapon.MagazineSize}");
        stats.AppendLine($"Перезарядка: {weapon.ReloadTime:F1} сек.");

        foreach (WeaponAffix affix in instance.Affixes)
        {
            if (affix == null)
                continue;

            string sign = affix.Value >= 0f ? "+" : "";

            string suffix =
                affix.ModifierType ==
                StatModifierType.Percent
                    ? "%"
                    : "";

            stats.AppendLine(
                $"{sign}{affix.Value:F1}{suffix} " +
                $"{affix.StatType}"
            );
        }

        statsText.text = stats.ToString();
        descriptionText.text = weapon.description;
        pickupText.text = "E — подобрать";
        Color rarityColor =
    GetRarityColor(instance.Rarity);

if (rarityFrame != null)
    rarityFrame.color = rarityColor;

if (background != null)
{
    background.color = Color.Lerp(
        new Color(0.04f, 0.04f, 0.05f, 0.95f),
        rarityColor,
        0.12f
    );
}

if (alignmentText != null)
{
    alignmentText.text =
        GetAlignmentName(instance.Alignment);

    alignmentText.color =
        instance.Alignment switch
        {
            ItemAlignment.Sanctified =>
                new Color(1f, 0.85f, 0.3f),

            ItemAlignment.Cursed =>
                new Color(0.7f, 0.2f, 1f),

            _ => Color.clear
        };
}
    }
public void SetTarget(Transform target)
{
    trackedTarget = target;
}

private void LateUpdate()
{
    if (trackedTarget == null ||
        !panel.activeSelf ||
        playerCamera == null)
    {
        return;
    }

    Vector3 screenPosition =
        playerCamera.WorldToScreenPoint(
            trackedTarget.position + worldOffset
        );

    if (screenPosition.z <= 0f)
    {
        panel.SetActive(false);
        return;
    }

    Vector2 position =
        new Vector2(
            screenPosition.x + screenOffset.x,
            screenPosition.y + screenOffset.y
        );

    float width = panelRect.rect.width;
    float height = panelRect.rect.height;

    float minimumX =
        width * panelRect.pivot.x;

    float maximumX =
        Screen.width -
        width * (1f - panelRect.pivot.x);

    float minimumY =
        height * panelRect.pivot.y;

    float maximumY =
        Screen.height -
        height * (1f - panelRect.pivot.y);

    position.x = Mathf.Clamp(
        position.x,
        minimumX,
        maximumX
    );

    position.y = Mathf.Clamp(
        position.y,
        minimumY,
        maximumY
    );

    panelRect.position = position;
}
    public void ShowItem(ItemData item, int amount)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        panel.SetActive(true);

        icon.sprite = item.icon;
        icon.enabled = item.icon != null;

        nameText.text = amount > 1
            ? $"{item.itemName} x{amount}"
            : item.itemName;

        rarityText.text = GetItemTypeName(item);
        rarityText.color = GetItemTypeColor(item);
        descriptionText.text = item.description;
        pickupText.text = "E — подобрать";

        StringBuilder stats = new StringBuilder();

        if (item is ConsumableData consumable &&
            consumable.HealAmount > 0f)
        {
            stats.AppendLine(
                $"Лечение: {consumable.HealAmount:F0}"
            );
        }

        if (item.effects != null)
        {
            foreach (StatEffect effect in item.effects)
            {
                if (effect != null)
                    stats.AppendLine(effect.EffectName);
            }
        }

        statsText.text = stats.ToString();
        Color itemColor = GetItemTypeColor(item);

if (rarityFrame != null)
    rarityFrame.color = itemColor;

if (background != null)
{
    background.color = Color.Lerp(
        new Color(0.04f, 0.04f, 0.05f, 0.95f),
        itemColor,
        0.1f
    );
}

if (alignmentText != null)
    alignmentText.text = "";
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
            trackedTarget = null;
    }

    private string GetRarityName(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Rare => "Редкое",
            ItemRarity.Legendary => "Легендарное",
            ItemRarity.Unique => "Уникальное",
            _ => "Обычное"
        };
    }

    private string GetAlignmentName(ItemAlignment alignment)
    {
        return alignment switch
        {
            ItemAlignment.Sanctified => "освящённое",
            ItemAlignment.Cursed => "проклятое",
            _ => ""
        };
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Rare =>
                new Color(0.2f, 0.5f, 1f),

            ItemRarity.Legendary =>
                new Color(1f, 0.45f, 0.05f),

            ItemRarity.Unique =>
                new Color(0.75f, 0.2f, 1f),

            _ => Color.white
        };
    }

    private string GetItemTypeName(ItemData item)
    {
        return item.itemType switch
        {
            ItemType.Passive => "Пассивный предмет",
            ItemType.Consumable => "Расходный предмет",
            ItemType.HealingFlask => "Лечение",
            ItemType.PathEffect =>
                item.pathEffectType == PathEffectType.Blessing
                    ? "Благословение"
                    : "Проклятие",

            _ => item.itemType.ToString()
        };
    }

    private Color GetItemTypeColor(ItemData item)
    {
        if (item.itemType == ItemType.PathEffect)
        {
            return item.pathEffectType ==
                   PathEffectType.Blessing
                ? new Color(1f, 0.85f, 0.3f)
                : new Color(0.65f, 0.15f, 0.8f);
        }

        return Color.white;
    }
}
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

    private void ShowItem(ItemData item)
    {
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
        statsText.text = BuildStatsText(item);
    }

    private string BuildStatsText(ItemData item)
    {
        if (item.effects == null ||
            item.effects.Length == 0)
        {
            return "Нет модификаторов";
        }

        StringBuilder builder = new StringBuilder();

        foreach (StatEffect effect in item.effects)
        {
            if (effect == null ||
                effect.Modifications == null)
            {
                continue;
            }

            foreach (StatEffectEntry entry
                     in effect.Modifications)
            {
                if (entry == null)
                    continue;

                string valueText;

                if (entry.modifierType ==
                    StatModifierType.Percent)
                {
                    float percent = entry.value * 100f;

                    valueText = percent >= 0f
                        ? $"+{percent:0.#}%"
                        : $"{percent:0.#}%";
                }
                else
                {
                    valueText = entry.value >= 0f
                        ? $"+{entry.value:0.#}"
                        : $"{entry.value:0.#}";
                }

                builder.AppendLine(
                    $"{entry.statType}: {valueText}"
                );
            }
        }

        return builder.Length > 0
            ? builder.ToString()
            : "Нет модификаторов";
    }
}
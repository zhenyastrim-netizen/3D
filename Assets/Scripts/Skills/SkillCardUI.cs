using TMPro;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour
{
    [Header("Skill")]
    [SerializeField] private SkillData skill;

    [Header("Player")]
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private PlayerSkillTree skillTree;

    [Header("UI")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text rankText;

    [Header("Layout")]
    [SerializeField] private bool configureLayoutAutomatically = true;
    [SerializeField] private Vector2 cardSize = new Vector2(300f, 210f);

    [Header("Colors")]
    [SerializeField] private Color availableColor =
        new Color32(26, 32, 48, 255);

    [SerializeField] private Color lockedColor =
        new Color32(35, 36, 42, 255);

    [SerializeField] private Color purchasedColor =
        new Color32(22, 55, 43, 255);

    private Image cardBackground;

    private void Awake()
    {
        if (experience == null)
            experience = FindFirstObjectByType<PlayerExperience>();

        if (skillTree == null)
            skillTree = FindFirstObjectByType<PlayerSkillTree>();

        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

        cardBackground = GetComponent<Image>();

        if (configureLayoutAutomatically)
            ConfigureLayout();

        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(TryPurchase);
    }

    private void OnEnable()
    {
        if (experience != null)
            experience.OnSkillPointsChanged += HandleSkillPointsChanged;

        if (skillTree != null)
            skillTree.OnSkillRankChanged += HandleSkillRankChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (experience != null)
            experience.OnSkillPointsChanged -= HandleSkillPointsChanged;

        if (skillTree != null)
            skillTree.OnSkillRankChanged -= HandleSkillRankChanged;
    }

    private void OnDestroy()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(TryPurchase);
    }

    private void TryPurchase()
    {
        if (skillTree == null || skill == null)
            return;

        skillTree.TryPurchase(skill);
        Refresh();
    }

    private void HandleSkillPointsChanged(int amount)
    {
        Refresh();
    }

    private void HandleSkillRankChanged(
    SkillData changedSkill,
    int newRank)
{
    Refresh();
}

    public void Refresh()
    {
        if (skill == null)
            return;

        int currentRank =
            skillTree != null
                ? skillTree.GetRank(skill)
                : 0;

        if (icon != null)
        {
            icon.sprite = skill.Icon;
            icon.enabled = skill.Icon != null;
        }

        if (nameText != null)
            nameText.text = skill.SkillName;

        if (descriptionText != null)
            descriptionText.text = skill.Description;

        if (rankText != null)
            rankText.text = $"{currentRank} / {skill.MaxRank}";

        bool isMaxRank = currentRank >= skill.MaxRank;
        bool requirementsMet =
            skillTree != null &&
            skillTree.MeetsRequirements(skill);

        if (costText != null)
        {
            if (isMaxRank)
            {
                costText.text = "Куплено";
            }
            else if (!requirementsMet && skill.HasRequirements)
            {
                costText.text = GetRequirementsText();
            }
            else
            {
                costText.text = $"Цена: {skill.Cost}";
            }
        }

        bool canAfford =
            experience != null &&
            experience.CanSpendSkillPoints(skill.Cost);

        if (purchaseButton != null)
        {
            purchaseButton.interactable =
                !isMaxRank &&
                requirementsMet &&
                canAfford;
        }

        if (cardBackground != null)
        {
            cardBackground.color = isMaxRank
                ? purchasedColor
                : requirementsMet
                    ? availableColor
                    : lockedColor;
        }
    }

    private string GetRequirementsText()
    {
        if (skill.Requirements != null && skill.Requirements.Count > 0)
        {
            StringBuilder text = new StringBuilder(
                skill.RequirementMode == SkillRequirementMode.Any
                    ? "Требуется любой: "
                    : "Требуются все: "
            );

            bool hasPrevious = false;

            foreach (SkillRequirement requirement in skill.Requirements)
            {
                if (requirement == null || requirement.Skill == null)
                    continue;

                if (hasPrevious)
                    text.Append(", ");

                text.Append(requirement.Skill.SkillName);

                if (requirement.RequiredRank > 1)
                    text.Append($" ({requirement.RequiredRank} ранг)");

                hasPrevious = true;
            }

            if (hasPrevious)
                return text.ToString();
        }

        if (skill.RequiredSkill != null)
        {
            return $"Требуется: {skill.RequiredSkill.SkillName} " +
                   $"ранг {skill.RequiredSkillRank}";
        }

        return $"Цена: {skill.Cost}";
    }

    private void ConfigureLayout()
    {
        RectTransform cardRect = transform as RectTransform;

        if (cardRect != null)
        {
            cardRect.localScale = Vector3.one;
            cardRect.sizeDelta = cardSize;
        }

        SetFixedRect(icon, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(16f, -16f), new Vector2(64f, 64f));

        SetTopStretch(nameText, 16f, 34f, 92f, 80f);
        SetTopRight(rankText, 16f, 64f, 28f);
        SetTopStretch(descriptionText, 92f, 72f, 16f, 16f);
        SetBottomStretch(costText, 14f, 28f, 16f, 16f);

        if (icon != null)
        {
            icon.preserveAspect = true;
            icon.color = Color.white;
            icon.raycastTarget = false;
        }

        ConfigureText(nameText, 22f, TextAlignmentOptions.TopLeft);
        ConfigureText(descriptionText, 15f, TextAlignmentOptions.TopLeft);
        ConfigureText(costText, 14f, TextAlignmentOptions.BottomLeft);
        ConfigureText(rankText, 14f, TextAlignmentOptions.TopRight);
    }

    private static void ConfigureText(
        TMP_Text text,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        text.rectTransform.localScale = Vector3.one;
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.color = new Color32(235, 238, 244, 255);
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }

    private static void SetFixedRect(
        Graphic graphic,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        if (graphic == null)
            return;

        RectTransform rect = graphic.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
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

    private static void SetTopRight(
        TMP_Text text,
        float top,
        float width,
        float height)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-16f, -top);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomStretch(
        TMP_Text text,
        float bottom,
        float height,
        float left,
        float right)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, bottom + height);
    }
}

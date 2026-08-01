using TMPro;
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

    private void Awake()
    {
        if (experience == null)
            experience = FindFirstObjectByType<PlayerExperience>();

        if (skillTree == null)
            skillTree = FindFirstObjectByType<PlayerSkillTree>();

        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

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
        if (changedSkill == skill)
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

        if (costText != null)
        {
            costText.text = isMaxRank
                ? "Куплено"
                : $"Цена: {skill.Cost}";
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable =
                skillTree != null &&
                skillTree.CanPurchase(skill);
        }
    }
}
using TMPro;
using UnityEngine;

public class SkillPointsUI : MonoBehaviour
{
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private TMP_Text skillPointsText;

    private void OnEnable()
    {
        if (experience == null)
            experience = FindFirstObjectByType<PlayerExperience>();

        if (experience == null || skillPointsText == null)
            return;

        experience.OnSkillPointsChanged += Refresh;
        Refresh(experience.SkillPoints);
    }

    private void OnDisable()
    {
        if (experience != null)
            experience.OnSkillPointsChanged -= Refresh;
    }

    private void Refresh(int amount)
    {
        skillPointsText.text = $"Очки навыков: {amount}";
    }
}
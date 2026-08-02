using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExperienceUI : MonoBehaviour
{
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text levelText;

    private void Start()
    {
        if (experience == null)
            experience = FindFirstObjectByType<PlayerExperience>();

        experience.OnExperienceChanged += Refresh;
        experience.OnLevelChanged += HandleLevelChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (experience == null)
            return;

        experience.OnExperienceChanged -= Refresh;
        experience.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(int newLevel)
    {
        Refresh();
    }

    public void SetVisible(bool visible)
    {
        if (experienceSlider != null)
            experienceSlider.gameObject.SetActive(visible);

        if (experienceText != null)
            experienceText.gameObject.SetActive(visible);

        if (levelText != null)
            levelText.gameObject.SetActive(visible);
    }

    private void Refresh()
    {
        experienceSlider.maxValue =
            experience.ExperienceRequired;

        experienceSlider.value =
            experience.CurrentExperience;

        experienceText.text =
            $"{experience.CurrentExperience} / " +
            $"{experience.ExperienceRequired}";

        levelText.text = $"LVL {experience.Level}";
    }
}

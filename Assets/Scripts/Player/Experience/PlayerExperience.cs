using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerExperience : MonoBehaviour
{
    [Header("Experience")]
    [SerializeField, Min(1)] private int level = 1;
    [SerializeField, Min(1)] private int baseExperienceRequired = 100;
    [SerializeField, Min(1f)] private float experienceGrowth = 1.25f;

    [Header("Stats per level")]
    [SerializeField] private float healthPerLevel = 5f;
    [SerializeField] private float meleeDamagePerLevel = 1f;
    [SerializeField] private float rangedDamagePerLevel = 1f;
    [SerializeField] private float magicDamagePerLevel = 1f;

    private PlayerStats playerStats;
    private int currentExperience;
    private int skillPoints;

    public int Level => level;
    public int CurrentExperience => currentExperience;
    public int ExperienceRequired => CalculateRequiredExperience(level);
    public int SkillPoints => skillPoints;

    public event Action OnExperienceChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnSkillPointsChanged;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        currentExperience += amount;

        while (currentExperience >= ExperienceRequired)
        {
            currentExperience -= ExperienceRequired;
            LevelUp();
        }

        OnExperienceChanged?.Invoke();
    }
    public bool CanSpendSkillPoints(int amount = 1)
{
    return amount > 0 && skillPoints >= amount;
}

public bool TrySpendSkillPoints(int amount = 1)
{
    if (!CanSpendSkillPoints(amount))
        return false;

    skillPoints -= amount;
    OnSkillPointsChanged?.Invoke(skillPoints);

    Debug.Log($"Потрачено очков навыков: {amount}. Осталось: {skillPoints}");

    return true;
}

    private void LevelUp()
    {
        level++;
        skillPoints++;

        IncreaseBaseStat(
            StatType.MaxHealth,
            healthPerLevel
        );

        IncreaseBaseStat(
            StatType.MeleeDamage,
            meleeDamagePerLevel
        );

        IncreaseBaseStat(
            StatType.RangedDamage,
            rangedDamagePerLevel
        );

        IncreaseBaseStat(
            StatType.MagicDamage,
            magicDamagePerLevel
        );

        OnLevelChanged?.Invoke(level);
        OnSkillPointsChanged?.Invoke(skillPoints);

        Debug.Log($"Новый уровень: {level}");
    }

    private void IncreaseBaseStat(
        StatType statType,
        float amount)
    {
        float currentBase =
            playerStats.GetBaseValue(statType);

        playerStats.SetBaseValue(
            statType,
            currentBase + amount
        );
    }

    private int CalculateRequiredExperience(int targetLevel)
    {
        return Mathf.RoundToInt(
            baseExperienceRequired *
            Mathf.Pow(experienceGrowth, targetLevel - 1)
        );
    }
}
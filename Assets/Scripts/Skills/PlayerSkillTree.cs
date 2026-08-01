using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerExperience))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerSkillTree : MonoBehaviour
{
    private PlayerExperience experience;
    private PlayerStats playerStats;

    private readonly Dictionary<SkillData, int> skillRanks =
        new Dictionary<SkillData, int>();

    private readonly Dictionary<SkillData, List<StatEffectInstance>> effects =
        new Dictionary<SkillData, List<StatEffectInstance>>();

    public event Action<SkillData, int> OnSkillRankChanged;

    private void Awake()
    {
        experience = GetComponent<PlayerExperience>();
        playerStats = GetComponent<PlayerStats>();
    }

    public int GetRank(SkillData skill)
    {
        if (skill == null)
            return 0;

        return skillRanks.TryGetValue(skill, out int rank)
            ? rank
            : 0;
    }

    public bool CanPurchase(SkillData skill)
    {
        if (skill == null || skill.StatEffect == null)
            return false;

        int currentRank = GetRank(skill);

        return currentRank < skill.MaxRank &&
               experience.CanSpendSkillPoints(skill.Cost);
    }

    public bool TryPurchase(SkillData skill)
    {
        if (!CanPurchase(skill))
            return false;

        StatEffectInstance effect =
            skill.StatEffect.Apply(playerStats);

        if (effect == null)
            return false;

        if (!experience.TrySpendSkillPoints(skill.Cost))
        {
            effect.Remove();
            return false;
        }

        int newRank = GetRank(skill) + 1;
        skillRanks[skill] = newRank;

        if (!effects.TryGetValue(
                skill,
                out List<StatEffectInstance> skillEffects))
        {
            skillEffects = new List<StatEffectInstance>();
            effects.Add(skill, skillEffects);
        }

        skillEffects.Add(effect);

        OnSkillRankChanged?.Invoke(skill, newRank);

        Debug.Log(
            $"Куплен навык: {skill.SkillName}, ранг: {newRank}"
        );

        return true;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerExperience))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCombatEvents))]
public class PlayerSkillTree : MonoBehaviour
{
    private PlayerExperience experience;
    private PlayerStats playerStats;

    private readonly Dictionary<SkillData, int> skillRanks =
        new Dictionary<SkillData, int>();

    private readonly Dictionary<SkillData, List<StatEffectInstance>> effects =
        new Dictionary<SkillData, List<StatEffectInstance>>();

    public event Action<SkillData, int> OnSkillRankChanged;

    public bool MeetsRequirements(SkillData skill)
    {
        if (skill == null)
            return false;

        bool hasNewRequirements = false;
        bool anyRequirementMet = false;

        if (skill.Requirements != null)
        {
            foreach (SkillRequirement requirement in skill.Requirements)
            {
                if (requirement == null || requirement.Skill == null)
                    continue;

                hasNewRequirements = true;
                bool isMet = GetRank(requirement.Skill) >=
                             requirement.RequiredRank;

                if (skill.RequirementMode == SkillRequirementMode.All && !isMet)
                    return false;

                if (isMet)
                    anyRequirementMet = true;
            }
        }

        if (hasNewRequirements)
        {
            return skill.RequirementMode == SkillRequirementMode.All ||
                   anyRequirementMet;
        }

        // Поддержка зависимостей, настроенных до появления списка.
        if (skill.RequiredSkill == null)
            return true;

        return GetRank(skill.RequiredSkill) >=
               skill.RequiredSkillRank;
    }

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
    if (skill == null)
        return false;

    int currentRank = GetRank(skill);

    return currentRank < skill.MaxRank &&
           MeetsRequirements(skill) &&
           experience.CanSpendSkillPoints(skill.Cost);
}
public bool HasSkill(SkillData skill)
{
    return GetRank(skill) > 0;
}
    public bool TryPurchase(SkillData skill)
    {
        if (!CanPurchase(skill))
            return false;

        int newRank = GetRank(skill) + 1;

        StatEffectInstance effect = null;

if (skill.StatEffect != null)
{
    effect = skill.StatEffect.Apply(playerStats);

    if (effect == null)
        return false;
}

if (!experience.TrySpendSkillPoints(skill.Cost))
{
    effect?.Remove();
    return false;
}

        skillRanks[skill] = newRank;

        if (skill.SkillEffect != null)
            skill.SkillEffect.Apply(gameObject, newRank);

        if (effect != null)
{
    if (!effects.TryGetValue(
            skill,
            out List<StatEffectInstance> skillEffects))
    {
        skillEffects = new List<StatEffectInstance>();
        effects.Add(skill, skillEffects);
    }

    skillEffects.Add(effect);
}

        OnSkillRankChanged?.Invoke(skill, newRank);

        Debug.Log(
            $"Куплен навык: {skill.SkillName}, ранг: {newRank}"
        );

        return true;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillRequirementMode
{
    Any,
    All
}

[Serializable]
public class SkillRequirement
{
    [SerializeField] private SkillData skill;
    [SerializeField, Min(1)] private int requiredRank = 1;

    public SkillData Skill => skill;
    public int RequiredRank => Mathf.Max(1, requiredRank);
}

[CreateAssetMenu(
    fileName = "New Skill",
    menuName = "Skills/Skill"
)]
public class SkillData : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string skillName;

    [TextArea(2, 5)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Requirements")]
    [Tooltip("Any: достаточно одного навыка. All: нужны все навыки.")]
    [SerializeField] private SkillRequirementMode requirementMode =
        SkillRequirementMode.Any;

    [SerializeField] private List<SkillRequirement> requirements =
        new List<SkillRequirement>();

    // Старые поля сохранены скрытыми, чтобы уже настроенные SkillData
    // не потеряли зависимость после обновления системы.
    [SerializeField, HideInInspector] private SkillData requiredSkill;
    [SerializeField, HideInInspector, Min(1)] private int requiredSkillRank = 1;

    [Header("Purchase")]
    [SerializeField, Min(1)] private int cost = 1;
    [SerializeField, Min(1)] private int maxRank = 1;

    [Header("Effect")]
    [SerializeField] private StatEffect statEffect;
    [SerializeField] private SkillEffect skillEffect;

    public string SkillName => skillName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public int MaxRank => maxRank;
    public StatEffect StatEffect => statEffect;
    public SkillEffect SkillEffect => skillEffect;
    public SkillData RequiredSkill => requiredSkill;
    public int RequiredSkillRank => requiredSkillRank;
    public SkillRequirementMode RequirementMode => requirementMode;
    public IReadOnlyList<SkillRequirement> Requirements => requirements;

    public bool HasRequirements
    {
        get
        {
            if (requiredSkill != null)
                return true;

            if (requirements == null)
                return false;

            foreach (SkillRequirement requirement in requirements)
            {
                if (requirement != null && requirement.Skill != null)
                    return true;
            }

            return false;
        }
    }
}

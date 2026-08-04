using UnityEngine;

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
[SerializeField] private SkillData requiredSkill;

[SerializeField, Min(1)]
private int requiredSkillRank = 1;

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
}
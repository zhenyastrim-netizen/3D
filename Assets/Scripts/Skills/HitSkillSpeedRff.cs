using UnityEngine;

[CreateAssetMenu(
    fileName = "Hit Speed Skill Effect",
    menuName = "Skills/Effects/Hit Speed"
)]
public class HitSpeedSkillEffect : SkillEffect
{
    [SerializeField] private int maxStacks = 3;
    [SerializeField] private float speedPerStack = 0.10f;
    [SerializeField] private float stackDuration = 5f;

    public override void Apply(GameObject player, int rank)
    {
        HitSpeedSkill skill = player.GetComponent<HitSpeedSkill>();

        if (skill == null)
            skill = player.AddComponent<HitSpeedSkill>();

        skill.Configure(maxStacks, speedPerStack, stackDuration, rank);
    }
}
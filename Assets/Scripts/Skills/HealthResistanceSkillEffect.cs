using UnityEngine;

[CreateAssetMenu(
    fileName = "Health Resistance Skill Effect",
    menuName = "Skills/Effects/Health Resistance"
)]
public class HealthResistanceSkillEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)]
    private float maxHealthToResistancePerRank = 0.05f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        HealthResistanceSkillRuntime runtime =
            player.GetComponent<HealthResistanceSkillRuntime>();

        if (runtime == null)
        {
            runtime =
                player.AddComponent<HealthResistanceSkillRuntime>();
        }

        runtime.Configure(
            rank,
            maxHealthToResistancePerRank
        );
    }
}

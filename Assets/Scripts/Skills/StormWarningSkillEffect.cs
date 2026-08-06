using UnityEngine;

[CreateAssetMenu(
    fileName = "Storm Warning Skill Effect",
    menuName = "Skills/Effects/Storm Warning"
)]
public class StormWarningSkillEffect : SkillEffect
{
    [SerializeField, Min(0f)]
    private float lightningDamagePerRank = 0.10f;

    [SerializeField, Min(0)]
    private int additionalJumpsPerRank = 1;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        ElementalSkillRuntime runtime =
            player.GetComponent<ElementalSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<ElementalSkillRuntime>();

        runtime.ConfigureStormWarning(
            rank,
            lightningDamagePerRank,
            additionalJumpsPerRank
        );
    }
}

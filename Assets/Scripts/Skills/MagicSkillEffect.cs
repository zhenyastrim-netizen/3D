using UnityEngine;

[CreateAssetMenu(
    fileName = "Magic Skill Effect",
    menuName = "Skills/Effects/Magic"
)]
public class MagicSkillEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)]
    private float manaCostReductionPerRank = 0.05f;

    [SerializeField, Range(0f, 1f)]
    private float freeCastChancePerRank = 0.05f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        MagicSkillRuntime runtime =
            player.GetComponent<MagicSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<MagicSkillRuntime>();

        runtime.ConfigureMagic(
            rank,
            manaCostReductionPerRank,
            freeCastChancePerRank
        );
    }
}

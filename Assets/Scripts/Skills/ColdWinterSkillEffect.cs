using UnityEngine;

[CreateAssetMenu(
    fileName = "Cold Winter Skill Effect",
    menuName = "Skills/Effects/Cold Winter"
)]
public class ColdWinterSkillEffect : SkillEffect
{
    [SerializeField, Min(0f)]
    private float frostDamagePerRank = 0.10f;

    [SerializeField, Range(0f, 1f)]
    private float armorReductionPerRank = 0.05f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        ElementalSkillRuntime runtime =
            player.GetComponent<ElementalSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<ElementalSkillRuntime>();

        runtime.ConfigureColdWinter(
            rank,
            frostDamagePerRank,
            armorReductionPerRank
        );
    }
}

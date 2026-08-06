using UnityEngine;

[CreateAssetMenu(
    fileName = "Elemental Bullets Skill Effect",
    menuName = "Skills/Effects/Elemental Bullets"
)]
public class ElementalBulletsSkillEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)]
    private float chancePerRank = 0.05f;

    [SerializeField, Min(0f)]
    private float elementalDamageMultiplier = 0.25f;

    [SerializeField, Min(0f)]
    private float buildupPerProc = 25f;

    [SerializeField, Min(0f)]
    private float statusDurationPerRank = 0.10f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        ElementalSkillRuntime runtime =
            GetOrAddRuntime(player);

        runtime.ConfigureElementalBullets(
            rank,
            chancePerRank,
            elementalDamageMultiplier,
            buildupPerProc,
            statusDurationPerRank
        );
    }

    private static ElementalSkillRuntime GetOrAddRuntime(
        GameObject player)
    {
        ElementalSkillRuntime runtime =
            player.GetComponent<ElementalSkillRuntime>();

        return runtime != null
            ? runtime
            : player.AddComponent<ElementalSkillRuntime>();
    }
}

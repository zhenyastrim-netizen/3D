using UnityEngine;

[CreateAssetMenu(
    fileName = "Shooting Skill Effect",
    menuName = "Skills/Effects/Shooting"
)]
public class ShootingSkillEffect : SkillEffect
{
    [SerializeField, Min(0f)]
    private float rangedDamagePerRank = 0.05f;

    [SerializeField, Min(0f)]
    private float secondHitDamagePerRank = 0.10f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        ShootingSkillRuntime runtime =
            player.GetComponent<ShootingSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<ShootingSkillRuntime>();

        runtime.Configure(
            rank,
            rangedDamagePerRank,
            secondHitDamagePerRank
        );
    }
}

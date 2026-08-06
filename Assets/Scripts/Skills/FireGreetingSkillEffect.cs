using UnityEngine;

[CreateAssetMenu(
    fileName = "Fire Greeting Skill Effect",
    menuName = "Skills/Effects/Fire Greeting"
)]
public class FireGreetingSkillEffect : SkillEffect
{
    [SerializeField, Min(0f)]
    private float fireDamagePerRank = 0.10f;

    [SerializeField, Min(0f)]
    private float burnDurationPerRank = 0.10f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        ElementalSkillRuntime runtime =
            player.GetComponent<ElementalSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<ElementalSkillRuntime>();

        runtime.ConfigureFireGreeting(
            rank,
            fireDamagePerRank,
            burnDurationPerRank
        );
    }
}

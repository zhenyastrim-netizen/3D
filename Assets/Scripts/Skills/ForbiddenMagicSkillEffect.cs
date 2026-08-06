using UnityEngine;

[CreateAssetMenu(
    fileName = "Forbidden Magic Skill Effect",
    menuName = "Skills/Effects/Forbidden Magic"
)]
public class ForbiddenMagicSkillEffect : SkillEffect
{
    [Header("Permanent bonuses")]
    [SerializeField, Min(0f)]
    private float magicDamageBonus = 0.50f;

    [SerializeField, Min(0f)]
    private float castSpeedBonus = 0.50f;

    [SerializeField, Min(0f)]
    private float manaCostIncrease = 0.50f;

    [Header("Forbidden Fruit")]
    [SerializeField, Range(0f, 1f)]
    private float fruitChance = 0.10f;

    [SerializeField, Min(1)]
    private int stacksToSummon = 10;

    [SerializeField, Min(0f)]
    private float damageTakenPerStack = 0.05f;

    [Header("Ghost")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField, Min(0f)] private float ghostMoveSpeed = 5f;
    [SerializeField, Min(0f)] private float ghostDamage = 10f;
    [SerializeField, Min(0.05f)] private float ghostAttackInterval = 1f;
    [SerializeField, Min(0f)] private float ghostAttackRange = 1.5f;

    [Header("Good From Evil")]
    [SerializeField, Min(0.1f)]
    private float buffDuration = 5f;

    [SerializeField, Min(0f)]
    private float buffMagicDamageBonus = 0.50f;

    [SerializeField, Min(0f)]
    private float buffStatusDurationBonus = 0.50f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        MagicSkillRuntime runtime =
            player.GetComponent<MagicSkillRuntime>();

        if (runtime == null)
            runtime = player.AddComponent<MagicSkillRuntime>();

        runtime.ConfigureForbiddenMagic(
            rank,
            magicDamageBonus,
            castSpeedBonus,
            manaCostIncrease,
            fruitChance,
            stacksToSummon,
            damageTakenPerStack,
            ghostPrefab,
            ghostMoveSpeed,
            ghostDamage,
            ghostAttackInterval,
            ghostAttackRange,
            buffDuration,
            buffMagicDamageBonus,
            buffStatusDurationBonus
        );
    }
}

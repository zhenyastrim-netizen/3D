using UnityEngine;

[CreateAssetMenu(
    fileName = "Teenage Crisis Skill Effect",
    menuName = "Skills/Effects/Teenage Crisis"
)]
public class TeenageCrisisSkillEffect : SkillEffect
{
    [Header("Risk and reward")]
    [SerializeField, Min(0f)]
    private float damageTakenIncrease = 0.25f;

    [SerializeField, Min(0f)]
    private float moveSpeedBonus = 0.20f;

    [SerializeField, Min(0f)]
    private float outgoingDamageBonus = 0.25f;

    [SerializeField, Range(0f, 1f)]
    private float criticalChanceBonus = 0.10f;

    [Header("Uncertainty")]
    [SerializeField, Min(1)]
    private int maxStacks = 6;

    [SerializeField, Min(0.1f)]
    private float sameWeaponResetTime = 3f;

    [SerializeField, Min(0.05f)]
    private float volleyInterval = 1f;

    [SerializeField, Range(0f, 2f)]
    private float bulletDamageMultiplier = 0.35f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0f)] private float bulletSpeed = 15f;
    [SerializeField, Min(0.1f)] private float bulletLifetime = 2f;
    [SerializeField, Min(0f)] private float bulletSpawnHeight = 1f;

    public override void Apply(GameObject player, int rank)
    {
        if (player == null)
            return;

        TeenageCrisisSkillRuntime runtime =
            player.GetComponent<TeenageCrisisSkillRuntime>();

        if (runtime == null)
        {
            runtime =
                player.AddComponent<TeenageCrisisSkillRuntime>();
        }

        runtime.Configure(
            rank,
            damageTakenIncrease,
            moveSpeedBonus,
            outgoingDamageBonus,
            criticalChanceBonus,
            maxStacks,
            sameWeaponResetTime,
            volleyInterval,
            bulletDamageMultiplier,
            bulletPrefab,
            bulletSpeed,
            bulletLifetime,
            bulletSpawnHeight
        );
    }
}

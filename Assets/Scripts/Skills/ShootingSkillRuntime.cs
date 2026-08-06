using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStats))]
public class ShootingSkillRuntime : MonoBehaviour
{
    private PlayerStats playerStats;
    private int rank;
    private float rangedDamagePerRank;
    private float secondHitDamagePerRank;
    private int hitCounter;

    public int HitCounter => hitCounter;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (rank > 0)
            RefreshDamageModifier();
    }

    public void Configure(
        int newRank,
        float newRangedDamagePerRank,
        float newSecondHitDamagePerRank)
    {
        rank = Mathf.Max(1, newRank);
        rangedDamagePerRank = Mathf.Max(0f, newRangedDamagePerRank);
        secondHitDamagePerRank =
            Mathf.Max(0f, newSecondHitDamagePerRank);

        RefreshDamageModifier();
    }

    public float ConsumeHitMultiplier()
    {
        if (rank <= 0)
            return 1f;

        hitCounter++;

        if (hitCounter < 2)
            return 1f;

        hitCounter = 0;
        return 1f + rank * secondHitDamagePerRank;
    }

    private void RefreshDamageModifier()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerStats.RemoveModifiersFromSource(this);

        playerStats.AddModifier(
            new StatModifier(
                StatType.RangedDamage,
                StatModifierType.Percent,
                rank * rangedDamagePerRank,
                this
            )
        );
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.RemoveModifiersFromSource(this);

        hitCounter = 0;
    }
}

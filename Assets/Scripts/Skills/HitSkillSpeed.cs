using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCombatEvents))]
public class HitSpeedSkill : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerCombatEvents combatEvents;
    private StatModifier speedModifier;

    private int maxStacks = 3;
    private float speedPerStack = 0.10f;
    private float stackDuration = 5f;
    private float remainingDuration;

    public int CurrentStacks { get; private set; }
    public int Rank { get; private set; }
    public float RemainingDuration => remainingDuration;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        combatEvents = GetComponent<PlayerCombatEvents>();
    }

    private void OnEnable()
    {
        combatEvents.OnEnemyHit += HandleEnemyHit;
    }

    private void OnDisable()
    {
        if (combatEvents != null)
            combatEvents.OnEnemyHit -= HandleEnemyHit;

        ClearStacks();
    }

    private void Update()
    {
        if (CurrentStacks <= 0)
            return;

        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
            ClearStacks();
    }

    public void Configure(
        int newMaxStacks,
        float newSpeedPerStack,
        float newStackDuration,
        int rank)
    {
        maxStacks = Mathf.Max(1, newMaxStacks);
        speedPerStack = Mathf.Max(0f, newSpeedPerStack);
        stackDuration = Mathf.Max(0.01f, newStackDuration);
        Rank = Mathf.Max(1, rank);

        CurrentStacks = Mathf.Min(CurrentStacks, maxStacks);
        RefreshModifier();
    }

    private void HandleEnemyHit(CombatHitInfo hitInfo)
    {
        // Цепные атаки и периодический урон не создают дополнительные стаки.
        if (hitInfo.IsSecondary)
            return;

        CurrentStacks = Mathf.Min(CurrentStacks + 1, maxStacks);
        remainingDuration = stackDuration;
        RefreshModifier();
    }

    private void RefreshModifier()
    {
        if (speedModifier != null)
        {
            playerStats.RemoveModifier(speedModifier);
            speedModifier = null;
        }

        if (CurrentStacks <= 0)
            return;

        speedModifier = new StatModifier(
            StatType.MoveSpeed,
            StatModifierType.Percent,
            speedPerStack * CurrentStacks,
            this
        );

        playerStats.AddModifier(speedModifier);
    }

    private void ClearStacks()
    {
        CurrentStacks = 0;
        remainingDuration = 0f;

        if (playerStats != null && speedModifier != null)
            playerStats.RemoveModifier(speedModifier);

        speedModifier = null;
    }
}
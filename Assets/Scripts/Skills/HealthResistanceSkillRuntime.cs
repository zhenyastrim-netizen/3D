using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStats))]
public class HealthResistanceSkillRuntime : MonoBehaviour
{
    private PlayerStats playerStats;
    private int rank;
    private float maxHealthToResistancePerRank;

    public float CurrentResistanceBonus { get; private set; }

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerStats.OnStatChanged += HandleStatChanged;

        if (rank > 0)
            RefreshResistance();
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= HandleStatChanged;
            playerStats.RemoveModifiersFromSource(this);
        }
    }

    public void Configure(int newRank, float healthPercentPerRank)
    {
        rank = Mathf.Max(1, newRank);
        maxHealthToResistancePerRank =
            Mathf.Clamp01(healthPercentPerRank);

        RefreshResistance();
    }

    private void HandleStatChanged(StatType statType, float newValue)
    {
        if (statType == StatType.MaxHealth)
            RefreshResistance();
    }

    private void RefreshResistance()
    {
        if (playerStats == null)
            return;

        playerStats.RemoveModifiersFromSource(this);

        CurrentResistanceBonus =
            playerStats.GetValue(StatType.MaxHealth) *
            maxHealthToResistancePerRank *
            rank;

        AddResistance(StatType.Armor);
        AddResistance(StatType.SpiritualDefense);
    }

    private void AddResistance(StatType statType)
    {
        playerStats.AddModifier(
            new StatModifier(
                statType,
                StatModifierType.Flat,
                CurrentResistanceBonus,
                this
            )
        );
    }
}

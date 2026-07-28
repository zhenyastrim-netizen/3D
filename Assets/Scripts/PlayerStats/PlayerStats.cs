using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Survival")]
    [SerializeField, Min(1f)]
    private float baseMaxHealth = 100f;

    [SerializeField, Min(0f)]
    private float baseArmor = 0f;

    [SerializeField, Min(0f)]
    private float baseHealingPower = 1f;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float baseMoveSpeed = 8f;

    [Header("Damage")]
    [SerializeField, Min(0f)]
    private float baseMeleeDamage = 10f;

    [SerializeField, Min(0f)]
    private float baseRangedDamage = 10f;

    [SerializeField, Min(0f)]
    private float baseMagicDamage = 10f;

    [Header("Combat speed")]
    [SerializeField, Min(0f)]
    private float baseAttackSpeed = 1f;

    [SerializeField, Min(0f)]
    private float baseReloadSpeed = 1f;

    [Header("Critical")]
    [SerializeField, Range(0f, 1f)]
    private float baseCriticalChance = 0.05f;

    [SerializeField, Min(1f)]
    private float baseCriticalDamage = 2f;

    [Header("Other")]
    [SerializeField, Min(0f)]
    private float baseLuck = 0f;

    private readonly Dictionary<StatType, PlayerStat> stats =
        new Dictionary<StatType, PlayerStat>();

    public event Action<StatType, float> OnStatChanged;
    public event Action OnAnyStatChanged;

    private void Awake()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        stats.Clear();

        stats.Add(
            StatType.MaxHealth,
            new PlayerStat(baseMaxHealth)
        );

        stats.Add(
            StatType.Armor,
            new PlayerStat(baseArmor)
        );

        stats.Add(
            StatType.HealingPower,
            new PlayerStat(baseHealingPower)
        );

        stats.Add(
            StatType.MoveSpeed,
            new PlayerStat(baseMoveSpeed)
        );

        stats.Add(
            StatType.MeleeDamage,
            new PlayerStat(baseMeleeDamage)
        );

        stats.Add(
            StatType.RangedDamage,
            new PlayerStat(baseRangedDamage)
        );

        stats.Add(
            StatType.MagicDamage,
            new PlayerStat(baseMagicDamage)
        );

        stats.Add(
            StatType.AttackSpeed,
            new PlayerStat(baseAttackSpeed)
        );

        stats.Add(
            StatType.ReloadSpeed,
            new PlayerStat(baseReloadSpeed)
        );

        stats.Add(
            StatType.CriticalChance,
            new PlayerStat(baseCriticalChance)
        );

        stats.Add(
            StatType.CriticalDamage,
            new PlayerStat(baseCriticalDamage)
        );

        stats.Add(
            StatType.Luck,
            new PlayerStat(baseLuck)
        );
    }

    public float GetValue(StatType statType)
    {
        if (!stats.TryGetValue(statType, out PlayerStat stat))
        {
            Debug.LogWarning(
                $"Характеристика {statType} не найдена в PlayerStats.",
                this
            );

            return 0f;
        }

        return stat.Value;
    }

    public float GetBaseValue(StatType statType)
    {
        if (!stats.TryGetValue(statType, out PlayerStat stat))
            return 0f;

        return stat.BaseValue;
    }

    public void SetBaseValue(
        StatType statType,
        float newValue)
    {
        if (!stats.TryGetValue(statType, out PlayerStat stat))
            return;

        stat.SetBaseValue(newValue);

        NotifyStatChanged(statType);
    }

    public bool AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            return false;

        if (!stats.TryGetValue(
                modifier.StatType,
                out PlayerStat stat))
        {
            Debug.LogWarning(
                $"Нельзя добавить модификатор: " +
                $"характеристика {modifier.StatType} не найдена.",
                this
            );

            return false;
        }

        stat.AddModifier(modifier);

        NotifyStatChanged(modifier.StatType);

        return true;
    }

    public bool RemoveModifier(StatModifier modifier)
    {
        if (modifier == null)
            return false;

        if (!stats.TryGetValue(
                modifier.StatType,
                out PlayerStat stat))
        {
            return false;
        }

        bool removed = stat.RemoveModifier(modifier);

        if (removed)
            NotifyStatChanged(modifier.StatType);

        return removed;
    }

    public int RemoveModifiersFromSource(object source)
    {
        if (source == null)
            return 0;

        int removedCount = 0;

        foreach (KeyValuePair<StatType, PlayerStat> pair in stats)
        {
            int removedFromStat =
                pair.Value.RemoveModifiersFromSource(source);

            if (removedFromStat <= 0)
                continue;

            removedCount += removedFromStat;

            NotifyStatChanged(pair.Key);
        }

        return removedCount;
    }

    private void NotifyStatChanged(StatType statType)
    {
        float newValue = GetValue(statType);

        OnStatChanged?.Invoke(statType, newValue);
        OnAnyStatChanged?.Invoke();
    }
}
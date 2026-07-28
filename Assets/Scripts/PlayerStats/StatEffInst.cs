using System.Collections.Generic;

public class StatEffectInstance
{
    private readonly PlayerStats targetStats;
    private readonly List<StatModifier> appliedModifiers;

    public StatEffect SourceEffect { get; }

    public bool IsActive { get; private set; }

    public StatEffectInstance(
        StatEffect sourceEffect,
        PlayerStats targetStats,
        List<StatModifier> appliedModifiers)
    {
        SourceEffect = sourceEffect;
        this.targetStats = targetStats;
        this.appliedModifiers = appliedModifiers;

        IsActive = true;
    }

    public void Remove()
    {
        if (!IsActive)
            return;

        if (targetStats != null)
        {
            foreach (StatModifier modifier in appliedModifiers)
            {
                targetStats.RemoveModifier(modifier);
            }
        }

        appliedModifiers.Clear();
        IsActive = false;
    }
}
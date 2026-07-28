using System;

[Serializable]
public class StatModifier
{
    public StatType StatType { get; }
    public StatModifierType ModifierType { get; }
    public float Value { get; }

    public object Source { get; }

    public StatModifier(
        StatType statType,
        StatModifierType modifierType,
        float value,
        object source)
    {
        StatType = statType;
        ModifierType = modifierType;
        Value = value;
        Source = source;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStat
{
    [SerializeField]
    private float baseValue;

    private readonly List<StatModifier> modifiers =
        new List<StatModifier>();

    public float BaseValue => baseValue;

    public float Value => CalculateFinalValue();

    public PlayerStat(float startingValue)
    {
        baseValue = startingValue;
    }

    public void SetBaseValue(float newValue)
    {
        baseValue = newValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            return;

        if (!modifiers.Contains(modifier))
            modifiers.Add(modifier);
    }

    public bool RemoveModifier(StatModifier modifier)
    {
        if (modifier == null)
            return false;

        return modifiers.Remove(modifier);
    }

    public int RemoveModifiersFromSource(object source)
    {
        if (source == null)
            return 0;

        return modifiers.RemoveAll(
            modifier => ReferenceEquals(modifier.Source, source)
        );
    }

    public void ClearModifiers()
    {
        modifiers.Clear();
    }

    private float CalculateFinalValue()
    {
        float flatTotal = 0f;
        float percentTotal = 0f;

        foreach (StatModifier modifier in modifiers)
        {
            switch (modifier.ModifierType)
            {
                case StatModifierType.Flat:
                    flatTotal += modifier.Value;
                    break;

                case StatModifierType.Percent:
                    percentTotal += modifier.Value;
                    break;
            }
        }

        float finalValue =
            (baseValue + flatTotal) *
            (1f + percentTotal);

        return Mathf.Max(0f, finalValue);
    }
}
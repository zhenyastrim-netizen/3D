using System;
using UnityEngine;

[Serializable]
public class StatEffectEntry
{
    [Tooltip("Характеристика, которую изменяет эффект")]
    public StatType statType;

    [Tooltip("Flat — обычное число. Percent — процент")]
    public StatModifierType modifierType;

    [Tooltip(
        "Для Flat: 10 означает +10.\n" +
        "Для Percent: 0.25 означает +25%, -0.15 означает -15%."
    )]
    public float value;
}
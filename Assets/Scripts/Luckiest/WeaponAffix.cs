using System;

[Serializable]
public class WeaponAffix
{
    private WeaponAffixDefinition definition;
    private float value;
    private bool isNegative;

    public WeaponAffixDefinition Definition =>
        definition;

    public StatType StatType =>
        definition.StatType;

    public StatModifierType ModifierType =>
        definition.ModifierType;

    public float Value => value;
    public bool IsNegative => isNegative;

    public WeaponAffix(
        WeaponAffixDefinition definition,
        float value,
        bool isNegative)
    {
        this.definition = definition;
        this.value = value;
        this.isNegative = isNegative;
    }
}
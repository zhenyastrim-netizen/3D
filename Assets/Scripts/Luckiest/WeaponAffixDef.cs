using UnityEngine;

[CreateAssetMenu(
    fileName = "New Weapon Affix",
    menuName = "Loot/Weapon Affix"
)]
public class WeaponAffixDefinition : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string affixName;

    [TextArea]
    [SerializeField] private string description;

    [Header("Modifier")]
    [SerializeField] private StatType statType;
    [SerializeField] private StatModifierType modifierType;

    [SerializeField] private float minimumValue;
    [SerializeField] private float maximumValue;

    [Header("Drop")]
    [SerializeField, Min(0.01f)]
    private float weight = 1f;

    [SerializeField]
    private WeaponType[] allowedWeaponTypes;

    public string AffixName => affixName;
    public string Description => description;
    public StatType StatType => statType;
    public StatModifierType ModifierType => modifierType;
    public float Weight => weight;
    [SerializeField]
private bool canBeNegative = true;

public bool CanBeNegative => canBeNegative;

    public bool CanRollFor(WeaponData weapon)
    {
        if (weapon == null)
            return false;

        if (allowedWeaponTypes == null ||
            allowedWeaponTypes.Length == 0)
        {
            return true;
        }

        foreach (WeaponType allowedType
                 in allowedWeaponTypes)
        {
            if (weapon.WeaponType == allowedType)
                return true;
        }

        return false;
    }

    public WeaponAffix Roll(bool negative = false)
    {
        float value = Random.Range(
            minimumValue,
            maximumValue
        );

        if (negative)
            value = -Mathf.Abs(value);

        return new WeaponAffix(
            this,
            value,
            negative
        );
    }
}
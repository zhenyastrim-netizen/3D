using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Weapon Affix Pool",
    menuName = "Loot/Weapon Affix Pool"
)]
public class WeaponAffixPool : ScriptableObject
{
    [SerializeField]
    private WeaponAffixDefinition[] affixes;

    public WeaponAffixDefinition Roll(
        WeaponData weapon,
        bool negative,
        HashSet<WeaponAffixDefinition> excluded)
    {
        List<WeaponAffixDefinition> available =
            new List<WeaponAffixDefinition>();

        float totalWeight = 0f;

        foreach (WeaponAffixDefinition affix in affixes)
        {
            if (affix == null)
                continue;

            if (excluded != null &&
                excluded.Contains(affix))
            {
                continue;
            }

            if (!affix.CanRollFor(weapon))
                continue;

            if (negative && !affix.CanBeNegative)
                continue;

            available.Add(affix);
            totalWeight += affix.Weight;
        }

        if (available.Count == 0)
            return null;

        float roll = Random.Range(0f, totalWeight);

        foreach (WeaponAffixDefinition affix in available)
        {
            roll -= affix.Weight;

            if (roll <= 0f)
                return affix;
        }

        return available[available.Count - 1];
    }
}
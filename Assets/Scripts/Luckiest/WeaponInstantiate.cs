using System;
using System.Collections.Generic;

[Serializable]
public class WeaponInstance
{
    private WeaponData baseData;
    private ItemRarity rarity;
    private ItemAlignment alignment;

    private readonly List<WeaponAffix> affixes =
        new List<WeaponAffix>();

    public WeaponData BaseData => baseData;
    public ItemRarity Rarity => rarity;
    public ItemAlignment Alignment => alignment;
    public IReadOnlyList<WeaponAffix> Affixes => affixes;

    public WeaponInstance(
        WeaponData baseData,
        ItemRarity rarity,
        ItemAlignment alignment)
    {
        this.baseData = baseData;
        this.rarity = rarity;
        this.alignment = alignment;
    }

    public void AddAffix(WeaponAffix affix)
    {
        if (affix != null)
            affixes.Add(affix);
    }
}
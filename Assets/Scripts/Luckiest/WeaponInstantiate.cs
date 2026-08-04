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

    private bool hasAmmoState;
    private int currentAmmo;
    private int reserveAmmo;
    private int magazineSize;

    public WeaponData BaseData => baseData;
    public ItemRarity Rarity => rarity;
    public ItemAlignment Alignment => alignment;
    public IReadOnlyList<WeaponAffix> Affixes => affixes;
    public bool HasAmmoState => hasAmmoState;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;

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

    public void SaveAmmoState(
        int newCurrentAmmo,
        int newReserveAmmo,
        int newMagazineSize)
    {
        currentAmmo = Math.Max(0, newCurrentAmmo);
        reserveAmmo = Math.Max(0, newReserveAmmo);
        magazineSize = Math.Max(1, newMagazineSize);
        hasAmmoState = true;
    }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Armor")]
public class ArmorData : ItemData
{
    [SerializeField] private int armor;

    public int Armor => armor;
}
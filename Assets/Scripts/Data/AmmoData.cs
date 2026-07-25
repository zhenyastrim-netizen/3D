using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Ammo")]
public class AmmoData : ItemData
{
    [SerializeField] private int amount;

    public int Amount => amount;
}
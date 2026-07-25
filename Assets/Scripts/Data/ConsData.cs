using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    [SerializeField] private float healAmount;

    public float HealAmount => healAmount;
}
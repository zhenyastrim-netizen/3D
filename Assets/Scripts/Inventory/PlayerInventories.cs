using UnityEngine;

public class PlayerInventories : MonoBehaviour
{
    [SerializeField] private Inventory mainInventory;
    [SerializeField] private Inventory passiveInventory;
    [SerializeField] private Inventory pathEffectInventory;
    [SerializeField] private Inventory questInventory;

    public Inventory Main => mainInventory;
    public Inventory Passive => passiveInventory;
    public Inventory PathEffects => pathEffectInventory;
    public Inventory Quest => questInventory;

    public Inventory GetTargetInventory(ItemData item)
    {
        if (item == null)
            return null;

        switch (item.itemType)
        {
            case ItemType.PathEffect:
                return pathEffectInventory;

            case ItemType.Quest:
                return questInventory;

            // Пассивка сначала попадает в сумку,
            // затем игрок переносит её в активный слот.
            case ItemType.Passive:
            case ItemType.Weapon:
            case ItemType.Consumable:
            case ItemType.HealingFlask:
            default:
                return mainInventory;
        }
    }
}
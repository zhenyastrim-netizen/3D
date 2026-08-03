using UnityEngine;

public class PlayerInventories : MonoBehaviour
{
    [SerializeField] private Inventory mainInventory;
    [SerializeField] private Inventory passiveInventory;
    [SerializeField] private Inventory pathEffectInventory;
    [SerializeField] private Inventory questInventory;
[SerializeField] private Inventory passiveEquipment;

public Inventory PassiveEquipment => passiveEquipment;
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
            case ItemType.Passive:
    return passiveInventory;

case ItemType.PathEffect:
    return pathEffectInventory;

case ItemType.Quest:
    return questInventory;

default:
    return mainInventory;
    
    }
    
}
}
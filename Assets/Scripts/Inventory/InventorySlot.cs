using System;

[Serializable]
public class InventorySlot
{
    private InventoryItem item;

    public InventoryItem Item => item;
    public bool IsEmpty => item == null;

    public void SetItem(InventoryItem inventoryItem)
    {
        item = inventoryItem;
    }

    public void Clear()
    {
        item = null;
    }

    public bool CanStack(ItemData other)
    {
        return !IsEmpty &&
               item.CanStackWith(other) &&
               !item.IsFull();
    }

    public void Swap(InventorySlot other)
    {
        if (other == null)
            return;

        InventoryItem temp = item;

        item = other.item;
        other.item = temp;
    }
}
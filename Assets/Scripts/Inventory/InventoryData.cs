using System;

[Serializable]
public class InventoryItem
{
    private ItemData item;
    private int amount;

    public ItemData Item => item;
    public int Amount => amount;

    public InventoryItem(ItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public bool CanStackWith(ItemData other)
    {
        return item == other && item.stackable;
    }

    public bool IsFull()
    {
        return amount >= item.maxStack;
    }

    public int AddAmount(int value)
    {
        if (!item.stackable)
            return value;

        int freeSpace = item.maxStack - amount;

        int toAdd = Math.Min(freeSpace, value);

        amount += toAdd;

        return value - toAdd;
    }

    public void RemoveAmount(int value)
    {
        amount -= value;

        if (amount < 0)
            amount = 0;
    }
}
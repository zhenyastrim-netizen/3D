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

        if (item == null)
        {
            this.amount = 0;
            return;
        }

        if (!item.stackable)
        {
            this.amount = 1;
            return;
        }

        this.amount = Math.Min(amount, item.maxStack);
    }

    public bool CanStackWith(ItemData other)
    {
        if (item == null || other == null)
            return false;

        return item == other &&
               item.stackable &&
               other.stackable;
    }

    public bool IsFull()
    {
        if (item == null)
            return true;

        if (!item.stackable)
            return true;

        return amount >= item.maxStack;
    }

    public int AddAmount(int value)
    {
        if (item == null)
            return value;

        if (!item.stackable)
            return value;

        if (value <= 0)
            return value;

        int freeSpace = item.maxStack - amount;
        int toAdd = Math.Min(freeSpace, value);

        amount += toAdd;

        return value - toAdd;
    }

    public void RemoveAmount(int value)
    {
        if (value <= 0)
            return;

        amount -= value;

        if (amount < 0)
            amount = 0;
    }
}
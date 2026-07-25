using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int slotCount = 24;

    public InventorySlot[] Slots { get; private set; }

    public event Action OnInventoryChanged;

    private void Awake()
    {
        Slots = new InventorySlot[slotCount];

        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = new InventorySlot();
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        // Сначала заполняем уже существующие стаки.
        if (item.stackable)
        {
            foreach (InventorySlot slot in Slots)
            {
                if (!slot.CanStack(item))
                    continue;

                remaining = slot.Item.AddAmount(remaining);

                if (remaining <= 0)
                {
                    NotifyChanged();
                    return true;
                }
            }
        }

        // Затем создаем новые стаки в пустых слотах.
        foreach (InventorySlot slot in Slots)
        {
            if (!slot.IsEmpty)
                continue;

            int amountForSlot = item.stackable
                ? Mathf.Min(remaining, item.maxStack)
                : 1;

            slot.SetItem(new InventoryItem(item, amountForSlot));
            remaining -= amountForSlot;

            if (remaining <= 0)
            {
                NotifyChanged();
                return true;
            }
        }

        // Часть предметов могла добавиться, даже если места на всё не хватило.
        if (remaining < amount)
            NotifyChanged();

        return remaining <= 0;
    }

    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
            return false;

        if (fromIndex == toIndex)
            return false;

        InventorySlot fromSlot = Slots[fromIndex];
        InventorySlot toSlot = Slots[toIndex];

        if (fromSlot.IsEmpty)
            return false;

        if (toSlot.IsEmpty)
        {
            toSlot.SetItem(fromSlot.Item);
            fromSlot.Clear();

            NotifyChanged();
            return true;
        }

        if (toSlot.CanStack(fromSlot.Item.Item))
        {
            int oldAmount = fromSlot.Item.Amount;
            int remaining = toSlot.Item.AddAmount(oldAmount);
            int movedAmount = oldAmount - remaining;

            if (movedAmount <= 0)
                return false;

            fromSlot.Item.RemoveAmount(movedAmount);

            if (fromSlot.Item.Amount <= 0)
                fromSlot.Clear();

            NotifyChanged();
            return true;
        }

        fromSlot.Swap(toSlot);

        NotifyChanged();
        return true;
    }

    public bool SplitStack(int fromIndex, int toIndex)
    {
        if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
            return false;

        if (fromIndex == toIndex)
            return false;

        InventorySlot fromSlot = Slots[fromIndex];
        InventorySlot toSlot = Slots[toIndex];

        if (fromSlot.IsEmpty || !toSlot.IsEmpty)
            return false;

        if (!fromSlot.Item.Item.stackable)
            return false;

        if (fromSlot.Item.Amount <= 1)
            return false;

        int splitAmount = fromSlot.Item.Amount / 2;

        fromSlot.Item.RemoveAmount(splitAmount);

        toSlot.SetItem(
            new InventoryItem(fromSlot.Item.Item, splitAmount)
        );

        NotifyChanged();
        return true;
    }

    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        if (!IsValidIndex(slotIndex) || amount <= 0)
            return false;

        InventorySlot slot = Slots[slotIndex];

        if (slot.IsEmpty)
            return false;

        slot.Item.RemoveAmount(amount);

        if (slot.Item.Amount <= 0)
            slot.Clear();

        NotifyChanged();
        return true;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Slots.Length;
    }

    private void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}
using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Min(1)]
    private int slotCount = 24;

    [Tooltip("Какие типы предметов разрешено класть в этот инвентарь")]
    [SerializeField]
    private ItemType[] allowedItemTypes;

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

    public bool CanAcceptItem(ItemData item)
    {
        if (item == null)
            return false;

        if (allowedItemTypes == null || allowedItemTypes.Length == 0)
            return true;

        foreach (ItemType allowedType in allowedItemTypes)
        {
            if (item.itemType == allowedType)
                return true;
        }

        return false;
    }
public bool AddWeapon(WeaponInstance weapon)
{
    if (weapon == null || weapon.BaseData == null)
        return false;

    if (!CanAcceptItem(weapon.BaseData))
        return false;

    foreach (InventorySlot slot in Slots)
    {
        if (!slot.IsEmpty)
            continue;

        slot.SetItem(
            new InventoryItem(weapon)
        );

        NotifyChanged();
        return true;
    }

    Debug.Log("В основном инвентаре нет места.");
    return false;
}
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        if (!CanAcceptItem(item))
        {
            Debug.LogWarning(
                $"Инвентарь {name} не принимает предмет типа {item.itemType}: {item.itemName}"
            );

            return false;
        }

        int remaining = amount;

        // Сначала заполняем существующие стаки.
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

        // Затем занимаем пустые слоты.
        foreach (InventorySlot slot in Slots)
        {
            if (!slot.IsEmpty)
                continue;

            int amountForSlot = item.stackable
                ? Mathf.Min(remaining, item.maxStack)
                : 1;

            slot.SetItem(
                new InventoryItem(item, amountForSlot)
            );

            remaining -= amountForSlot;

            if (remaining <= 0)
            {
                NotifyChanged();
                return true;
            }
        }

        // Даже если поместилась только часть предметов,
        // интерфейс должен обновиться.
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
    public bool TransferItemTo(
    int fromIndex,
    Inventory targetInventory,
    int targetIndex)
{
    if (targetInventory == null)
        return false;

    if (!IsValidIndex(fromIndex))
        return false;

    if (!targetInventory.IsValidIndex(targetIndex))
        return false;

    // Если это один и тот же инвентарь,
    // используем обычное перемещение.
    if (targetInventory == this)
        return MoveItem(fromIndex, targetIndex);

    InventorySlot fromSlot = Slots[fromIndex];
    InventorySlot targetSlot = targetInventory.Slots[targetIndex];

    if (fromSlot.IsEmpty)
        return false;

    ItemData draggedItemData = fromSlot.Item.Item;

    // Проверяем, принимает ли целевой инвентарь этот тип предмета.
    if (!targetInventory.CanAcceptItem(draggedItemData))
        return false;

    // Целевой слот пустой.
    if (targetSlot.IsEmpty)
    {
        targetSlot.SetItem(fromSlot.Item);
        fromSlot.Clear();

        NotifyChanged();
        targetInventory.NotifyChanged();

        return true;
    }

    // В целевом слоте такой же стакающийся предмет.
    if (targetSlot.CanStack(draggedItemData))
    {
        int oldAmount = fromSlot.Item.Amount;

        int remaining =
            targetSlot.Item.AddAmount(oldAmount);

        int movedAmount =
            oldAmount - remaining;

        if (movedAmount <= 0)
            return false;

        fromSlot.Item.RemoveAmount(movedAmount);

        if (fromSlot.Item.Amount <= 0)
            fromSlot.Clear();

        NotifyChanged();
        targetInventory.NotifyChanged();

        return true;
    }

    // Если предметы разные — пробуем поменять их местами.
    ItemData targetItemData = targetSlot.Item.Item;

    // Исходный инвентарь должен принять предмет из целевого слота.
    if (!CanAcceptItem(targetItemData))
        return false;

    fromSlot.Swap(targetSlot);

    NotifyChanged();
    targetInventory.NotifyChanged();

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

    public InventoryItem GetItem(int slotIndex)
    {
        if (!IsValidIndex(slotIndex))
            return null;

        return Slots[slotIndex].Item;
    }

    public void ForceRefresh()
    {
        NotifyChanged();
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
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public static event Action<ItemData> OnItemHovered;
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private GameObject selectionFrame;

    private InventorySlot slot;
    private Inventory inventory;
    private int slotIndex;
    

    public int SlotIndex => slotIndex;
    public Inventory Inventory => inventory;
    public Image Icon => icon;

    public void Setup(
        InventorySlot inventorySlot,
        int index,
        Inventory ownerInventory)
    {
        slot = inventorySlot;
        slotIndex = index;
        inventory = ownerInventory;

        SetSelected(false);
        Refresh();
    }
public void OnPointerEnter(PointerEventData eventData)
{
    if (slot == null || slot.IsEmpty)
        return;

    OnItemHovered?.Invoke(slot.Item.Item);
}

public void OnPointerExit(PointerEventData eventData)
{
    OnItemHovered?.Invoke(null);
}
    public void Refresh()
    {
        if (slot == null || slot.IsEmpty)
        {
            if (icon != null)
            {
                icon.enabled = false;
                icon.sprite = null;
            }

            if (amountText != null)
            {
                amountText.gameObject.SetActive(false);
                amountText.text = string.Empty;
            }

            return;
        }

        ItemData itemData = slot.Item.Item;

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = itemData.icon;
        }

        bool showAmount =
            itemData.stackable &&
            slot.Item.Amount > 1;

        if (amountText != null)
        {
            amountText.gameObject.SetActive(showAmount);
            amountText.text = showAmount
                ? slot.Item.Amount.ToString()
                : string.Empty;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(selected);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnItemHovered?.Invoke(null);
        if (slot == null || slot.IsEmpty)
            return;

        if (InventoryDragManager.Instance == null)
            return;

        InventoryDragManager.Instance.BeginDrag(this);

        if (icon != null)
            icon.enabled = false;

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Перетаскиваемая иконка двигается через InventoryDragManager.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryDragManager.Instance != null)
            InventoryDragManager.Instance.EndDrag();

        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryDragManager.Instance == null)
            return;

        InventorySlotUI draggedSlot =
            InventoryDragManager.Instance.DraggedSlot;

        if (draggedSlot == null)
            return;

        if (draggedSlot == this)
            return;

        if (draggedSlot.Inventory == null || inventory == null)
            return;

        draggedSlot.Inventory.TransferItemTo(
            draggedSlot.SlotIndex,
            inventory,
            slotIndex
        );
    }
}
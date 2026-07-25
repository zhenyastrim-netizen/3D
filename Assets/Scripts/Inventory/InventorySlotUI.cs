using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

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

        Refresh();
    }

    public void Refresh()
    {
        if (slot == null || slot.IsEmpty)
        {
            icon.enabled = false;
            icon.sprite = null;

            if (amountText != null)
                amountText.gameObject.SetActive(false);

            return;
        }

        icon.enabled = true;
        icon.sprite = slot.Item.Item.icon;

        bool showAmount =
            slot.Item.Item.stackable &&
            slot.Item.Amount > 1;

        if (amountText != null)
        {
            amountText.gameObject.SetActive(showAmount);
            amountText.text = slot.Item.Amount.ToString();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty)
            return;

        InventoryDragManager.Instance.BeginDrag(this);

        icon.enabled = false;

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Сам DragIcon двигается внутри InventoryDragManager.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryDragManager.Instance.EndDrag();

        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot =
            InventoryDragManager.Instance.DraggedSlot;

        if (draggedSlot == null)
            return;

        if (draggedSlot == this)
            return;

        if (draggedSlot.Inventory != inventory)
            return;

        inventory.MoveItem(
            draggedSlot.SlotIndex,
            slotIndex
        );
    }
}
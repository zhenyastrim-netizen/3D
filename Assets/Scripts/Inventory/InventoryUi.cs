using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;

    private InventorySlotUI[] slotViews;

    private void Start()
    {
        CreateSlots();

        inventory.OnInventoryChanged += RefreshAll;
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshAll;
    }

    private void CreateSlots()
    {
        slotViews = new InventorySlotUI[inventory.Slots.Length];

        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            InventorySlotUI slotUI =
                Instantiate(slotPrefab, slotParent);

            slotUI.Setup(inventory.Slots[i], i, inventory);

            slotViews[i] = slotUI;
        }
    }

    private void RefreshAll()
    {
        if (slotViews == null)
            return;

        foreach (InventorySlotUI slotUI in slotViews)
        {
            slotUI.Refresh();
        }
    }
}
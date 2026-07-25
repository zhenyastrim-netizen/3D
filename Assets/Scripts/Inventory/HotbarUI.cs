using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private HotbarController hotbarController;

    [Header("Settings")]
    [SerializeField, Min(1)] private int hotbarSize = 5;

    private InventorySlotUI[] slotViews;

    private void Start()
    {
        CreateSlots();

        inventory.OnInventoryChanged += RefreshAll;
        hotbarController.OnSelectedSlotChanged += UpdateSelection;

        RefreshAll();
        UpdateSelection(hotbarController.SelectedIndex);
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshAll;

        if (hotbarController != null)
            hotbarController.OnSelectedSlotChanged -= UpdateSelection;
    }

    private void CreateSlots()
    {
        int size = Mathf.Min(hotbarSize, inventory.Slots.Length);

        slotViews = new InventorySlotUI[size];

        for (int i = 0; i < size; i++)
        {
            InventorySlotUI slotUI =
                Instantiate(slotPrefab, slotParent);

            slotUI.Setup(
                inventory.Slots[i],
                i,
                inventory
            );

            slotViews[i] = slotUI;
        }
    }

    private void RefreshAll()
    {
        if (slotViews == null)
            return;

        foreach (InventorySlotUI slotUI in slotViews)
        {
            if (slotUI != null)
                slotUI.Refresh();
        }
    }

    private void UpdateSelection(int selectedIndex)
    {
        if (slotViews == null)
            return;

        for (int i = 0; i < slotViews.Length; i++)
        {
            slotViews[i].SetSelected(i == selectedIndex);
        }
    }
}
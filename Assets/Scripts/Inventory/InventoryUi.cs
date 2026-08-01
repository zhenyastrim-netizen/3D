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

        if (inventory == null)
            return;

        inventory.OnInventoryChanged += RefreshAll;
        RefreshAll();

        InventoryScrollView scrollView =
            GetComponent<InventoryScrollView>();

        if (scrollView != null)
            scrollView.RefreshLayout();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshAll;
    }

    private void CreateSlots()
    {
        if (inventory == null ||
            slotPrefab == null ||
            slotParent == null)
        {
            Debug.LogError(
                "InventoryUI is not fully configured.",
                this
            );

            return;
        }

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

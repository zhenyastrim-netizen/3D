using System.Collections.Generic;
using UnityEngine;

public class InventoryGridUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Inventory inventory;

    [Header("UI")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsContainer;

    private readonly List<InventorySlotUI> slotsUI =
        new List<InventorySlotUI>();

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;
    }

    private void Start()
    {
        Build();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
    }

    private void Build()
    {
        if (inventory == null ||
            inventory.Slots == null ||
            slotPrefab == null ||
            slotsContainer == null)
        {
            Debug.LogError(
                "InventoryGridUI настроен не полностью.",
                this
            );

            return;
        }

        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        slotsUI.Clear();

        for (int i = 0;
             i < inventory.Slots.Length;
             i++)
        {
            InventorySlotUI slotUI = Instantiate(
                slotPrefab,
                slotsContainer
            );

            slotUI.Setup(
                inventory.Slots[i],
                i,
                inventory
            );

            slotsUI.Add(slotUI);
        }
    }

    private void Refresh()
    {
        foreach (InventorySlotUI slotUI in slotsUI)
            slotUI.Refresh();
    }
}
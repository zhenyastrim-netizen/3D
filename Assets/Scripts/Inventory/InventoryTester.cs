using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    [Header("Test items")]
    [SerializeField] private ItemData firstItem;
    [SerializeField] private ItemData secondItem;

    private void Start()
    {
        if (firstItem != null)
            inventory.AddItem(firstItem, 1);

        if (secondItem != null)
            inventory.AddItem(secondItem, 25);
    }
}
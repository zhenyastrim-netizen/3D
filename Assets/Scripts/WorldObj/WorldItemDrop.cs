using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldItemDrop : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction pickupAction;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private TMP_Text itemNameText;

    [Header("Pickup")]
    [SerializeField, Min(0f)]
    private float pickupDelay = 0.5f;

    private ItemData item;
    private int amount;
    private PlayerInventories playerInventories;
    private float pickupAvailableTime;
    private bool playerInRange;

    public ItemData Item => item;
    public int Amount => amount;

    public void Initialize(
        ItemData newItem,
        int newAmount = 1)
    {
        item = newItem;
        amount = Mathf.Max(1, newAmount);

        if (item == null)
        {
            Destroy(gameObject);
            return;
        }

        SpawnModel();

        if (itemNameText != null)
        {
            itemNameText.text = amount > 1
                ? $"{item.itemName} x{amount}"
                : item.itemName;
        }

        pickupAvailableTime =
            Time.time + pickupDelay;
    }

    private void OnEnable()
    {
        pickupAction.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
    }

    private void Update()
    {
        if (!playerInRange ||
            Time.time < pickupAvailableTime)
        {
            return;
        }

        if (pickupAction.WasPressedThisFrame())
            TryPickup();
    }

    private void SpawnModel()
    {
        if (item.worldPrefab == null ||
            visualRoot == null)
        {
            return;
        }

        GameObject model = Instantiate(
            item.worldPrefab,
            visualRoot
        );

        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
    }

    private void TryPickup()
    {
        Inventory inventory =
            playerInventories.GetTargetInventory(item);

        if (inventory == null)
            return;

        if (inventory.AddItem(item, amount))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventories inventories =
            other.GetComponentInParent<PlayerInventories>();

        if (inventories == null)
            return;

        playerInventories = inventories;
        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInventories inventories =
            other.GetComponentInParent<PlayerInventories>();

        if (inventories != playerInventories)
            return;

        playerInventories = null;
        playerInRange = false;
    }
}
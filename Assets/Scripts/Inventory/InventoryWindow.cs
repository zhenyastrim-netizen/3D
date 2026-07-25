using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InputAction toggleInventory;

    private bool isOpen;

    private void OnEnable()
    {
        toggleInventory.Enable();
        toggleInventory.performed += Toggle;
    }

    private void OnDisable()
    {
        toggleInventory.performed -= Toggle;
        toggleInventory.Disable();
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        isOpen = false;
    }

    private void Toggle(InputAction.CallbackContext context)
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}
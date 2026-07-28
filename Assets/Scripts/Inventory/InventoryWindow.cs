using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryWindow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject playerMenu;

    [Header("Input")]
    [SerializeField] private InputAction toggleMenu;

    [Header("Player")]
    [SerializeField] private PlayerLook playerLook;

    [Header("Settings")]
    [SerializeField] private bool pauseGame = true;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (playerLook == null)
        {
            playerLook =
                FindFirstObjectByType<PlayerLook>();
        }
    }

    private void OnEnable()
    {
        toggleMenu.Enable();
        toggleMenu.performed += Toggle;
    }

    private void OnDisable()
    {
        toggleMenu.performed -= Toggle;
        toggleMenu.Disable();

        SetOpen(false);
    }

    private void Start()
    {
        SetOpen(false);
    }

    private void Toggle(
        InputAction.CallbackContext context)
    {
        SetOpen(!IsOpen);
    }

    public void SetOpen(bool open)
    {
        IsOpen = open;

        if (playerMenu != null)
            playerMenu.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        if (playerLook != null)
            playerLook.CanLook = !open;

        if (pauseGame)
            Time.timeScale = open ? 0f : 1f;
    }
}
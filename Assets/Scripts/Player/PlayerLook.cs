using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;

    [Header("Input")]
    [SerializeField] private InputAction lookAction;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float maxLookAngle = 85f;
    public bool CanLook { get; set; } = true;
    private float xRotation;

    void OnEnable()
    {
        lookAction.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        lookAction.Disable();
    }

    void Update()
    {
        if (!CanLook)
        return;
        Vector2 mouse = lookAction.ReadValue<Vector2>();

        float mouseX = mouse.x * sensitivity;
        float mouseY = mouse.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }
}
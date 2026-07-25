using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Position Sway")]
    [SerializeField] private float positionAmount = 0.0025f;
    [SerializeField] private float maxPositionAmount = 0.05f;

    [Header("Rotation Sway")]
    [SerializeField] private float rotationAmount = 0.12f;
    [SerializeField] private float maxRotationAmount = 5f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 12f;
    [SerializeField] private float rotationSmoothSpeed = 12f;

    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();
    }

    private void Update()
    {
        Vector2 lookInput = Vector2.zero;

        if (lookAction != null)
            lookInput = lookAction.action.ReadValue<Vector2>();

        float moveX = Mathf.Clamp(
            -lookInput.x * positionAmount,
            -maxPositionAmount,
            maxPositionAmount
        );

        float moveY = Mathf.Clamp(
            -lookInput.y * positionAmount,
            -maxPositionAmount,
            maxPositionAmount
        );

        Vector3 targetPosition =
            defaultLocalPosition +
            new Vector3(moveX, moveY, 0f);

        float rotationX = Mathf.Clamp(
            -lookInput.y * rotationAmount,
            -maxRotationAmount,
            maxRotationAmount
        );

        float rotationY = Mathf.Clamp(
            lookInput.x * rotationAmount,
            -maxRotationAmount,
            maxRotationAmount
        );

        float rotationZ = Mathf.Clamp(
            lookInput.x * rotationAmount * 0.5f,
            -maxRotationAmount,
            maxRotationAmount
        );

        Quaternion targetRotation =
            defaultLocalRotation *
            Quaternion.Euler(
                rotationX,
                rotationY,
                rotationZ
            );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            1f - Mathf.Exp(
                -positionSmoothSpeed * Time.deltaTime
            )
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            1f - Mathf.Exp(
                -rotationSmoothSpeed * Time.deltaTime
            )
        );
    }
}
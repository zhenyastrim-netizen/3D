using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private InputActionReference moveAction;

    [Header("Walk Bob")]
    [SerializeField] private float walkFrequency = 7f;
    [SerializeField] private float walkHorizontalAmplitude = 0.025f;
    [SerializeField] private float walkVerticalAmplitude = 0.035f;

    [Header("Air Bob")]
    [SerializeField] private float airTilt = 2f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 12f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;

    private float bobTime;

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        if (controller == null || moveAction == null)
        {
            ReturnToDefault();
            return;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        bool isMoving =
            input.sqrMagnitude > 0.01f &&
            controller.isGrounded &&
            controller.velocity.sqrMagnitude > 0.1f;

        Vector3 targetPosition = defaultLocalPosition;
        Quaternion targetRotation = defaultLocalRotation;

        if (isMoving)
        {
            float speedMultiplier = Mathf.Clamp(
                controller.velocity.magnitude / 5f,
                0.5f,
                2f
            );

            bobTime += Time.deltaTime *
                       walkFrequency *
                       speedMultiplier;

            float horizontal =
                Mathf.Cos(bobTime) *
                walkHorizontalAmplitude;

            float vertical =
                Mathf.Abs(Mathf.Sin(bobTime)) *
                walkVerticalAmplitude;

            targetPosition += new Vector3(
                horizontal,
                -vertical,
                0f
            );

            targetRotation *= Quaternion.Euler(
                vertical * 40f,
                horizontal * 25f,
                -horizontal * 80f
            );
        }
        else
        {
            bobTime = 0f;
        }

        if (!controller.isGrounded)
        {
            float verticalVelocity = controller.velocity.y;

            targetRotation *= Quaternion.Euler(
                Mathf.Clamp(-verticalVelocity, -airTilt, airTilt),
                0f,
                0f
            );
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            1f - Mathf.Exp(-positionSmoothSpeed * Time.deltaTime)
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime)
        );
    }

    private void ReturnToDefault()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            defaultLocalPosition,
            1f - Mathf.Exp(-positionSmoothSpeed * Time.deltaTime)
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            defaultLocalRotation,
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime)
        );
    }
}
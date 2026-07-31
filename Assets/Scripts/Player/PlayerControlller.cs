using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallbackMoveSpeed = 8f;

    [Tooltip("Во сколько раз спринт быстрее обычного движения")]
    [SerializeField] private float sprintMultiplier = 1.25f;

    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;

    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction sprintAction;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerCameraEffects cameraEffects;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private PlayerSlide playerSlide;

    private Vector3 currentVelocity;

    public Vector3 MoveDirection { get; private set; }

    public bool IsSprinting { get; private set; }

    public float CurrentMoveSpeed { get; private set; }

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (playerDash == null)
            playerDash = GetComponent<PlayerDash>();

        if (playerSlide == null)
            playerSlide = GetComponent<PlayerSlide>();

        if (motor == null)
        {
            Debug.LogError(
                "PlayerMovement: PlayerMotor не найден.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (playerSlide != null && playerSlide.IsSliding)
        {
            currentVelocity = Vector3.zero;
            motor.HorizontalVelocity = Vector3.zero;

            IsSprinting = false;
            UpdateSprintCamera(false);

            return;
        }

        if (playerDash != null && playerDash.IsDashing)
        {
            motor.HorizontalVelocity = Vector3.zero;

            IsSprinting = false;
            UpdateSprintCamera(false);

            return;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 direction =
            transform.forward * input.y +
            transform.right * input.x;

        direction.y = 0f;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        MoveDirection = direction;

        bool hasMovementInput =
            input.sqrMagnitude > 0.01f;

        IsSprinting =
            sprintAction.IsPressed() &&
            hasMovementInput;

        float baseSpeed = GetMoveSpeed();

        CurrentMoveSpeed = IsSprinting
            ? baseSpeed * sprintMultiplier
            : baseSpeed;

        UpdateSprintCamera(IsSprinting);

        Vector3 targetVelocity =
            direction * CurrentMoveSpeed;

        currentVelocity = targetVelocity;

        motor.HorizontalVelocity = currentVelocity;
    }

    private float GetMoveSpeed()
    {
        if (playerStats != null)
        {
            return playerStats.GetValue(
                StatType.MoveSpeed
            );
        }

        return fallbackMoveSpeed;
    }

    private void UpdateSprintCamera(bool isSprinting)
    {
        if (cameraEffects != null)
            cameraEffects.IsSprinting = isSprinting;
    }
}
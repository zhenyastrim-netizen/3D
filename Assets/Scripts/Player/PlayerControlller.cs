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
    [Header("Air movement")]
[SerializeField] private float airAcceleration = 8f;
[SerializeField] private float maximumBunnyHopSpeed = 25f;
[SerializeField] private PlayerGround playerGround;
[Header("Ground momentum")]
[SerializeField, Min(0f)]
private float groundFriction = 20f;

    private Vector3 currentVelocity;

    public Vector3 MoveDirection { get; private set; }

    public bool IsSprinting { get; private set; }

    public float CurrentMoveSpeed { get; private set; }

    private void Awake()
    {
        if (playerGround == null)
    playerGround = GetComponent<PlayerGround>();
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
    if (playerSlide != null &&
        playerSlide.IsSliding)
    {
        currentVelocity = Vector3.zero;
        motor.HorizontalVelocity = Vector3.zero;

        IsSprinting = false;
        UpdateSprintCamera(false);
        return;
    }

    if (playerDash != null &&
        playerDash.IsDashing)
    {
        currentVelocity = Vector3.zero;
        motor.HorizontalVelocity = Vector3.zero;

        IsSprinting = false;
        UpdateSprintCamera(false);
        return;
    }

    Vector2 input =
        moveAction.ReadValue<Vector2>();

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

    bool isGrounded =
        playerGround != null &&
        playerGround.IsGrounded;

    if (!isGrounded)
    {
        currentVelocity = Vector3.zero;
        motor.HorizontalVelocity = Vector3.zero;

        ApplyAirMovement(
            direction,
            hasMovementInput
        );

        return;
    }

    Vector3 momentum =
    motor.MomentumVelocity;

// На земле постепенно гасим накопленную инерцию.
momentum = Vector3.MoveTowards(
    momentum,
    Vector3.zero,
    groundFriction * Time.deltaTime
);

motor.SetMomentum(momentum);

float momentumSpeed = momentum.magnitude;

// Пока инерция быстрее обычного бега,
// PlayerMotor продолжает двигать персонажа ею.
if (momentumSpeed > CurrentMoveSpeed)
{
    currentVelocity = Vector3.zero;
    motor.HorizontalVelocity = Vector3.zero;
    return;
}

// Когда инерция стала достаточно маленькой,
// передаём движение обычному управлению.
motor.ClearMomentum();

    Vector3 targetVelocity =
        direction * CurrentMoveSpeed;

    currentVelocity = targetVelocity;
    motor.HorizontalVelocity = targetVelocity;
}
    private void ApplyAirMovement(
    Vector3 direction,
    bool hasInput)
{
    if (!hasInput)
        return;

    Vector3 velocity =
        motor.MomentumVelocity;

    Vector3 wishDirection =
        direction.normalized;

    float currentSpeed = Vector3.Dot(
        velocity,
        wishDirection
    );

    float speedToAdd =
        CurrentMoveSpeed - currentSpeed;

    if (speedToAdd <= 0f)
        return;

    float accelerationSpeed =
        airAcceleration *
        CurrentMoveSpeed *
        Time.deltaTime;

    accelerationSpeed = Mathf.Min(
        accelerationSpeed,
        speedToAdd
    );

    velocity +=
        wishDirection * accelerationSpeed;

    if (velocity.magnitude >
        maximumBunnyHopSpeed)
    {
        velocity =
            velocity.normalized *
            maximumBunnyHopSpeed;
    }

    motor.SetMomentum(velocity);
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
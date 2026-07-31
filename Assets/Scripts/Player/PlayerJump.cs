using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction jumpAction;

    [Header("Jump")]
    [SerializeField, Min(1)] private int maximumJumps = 1;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Wall jump")]
    [SerializeField, Min(0)] private int maximumWallJumps = 1;
    [SerializeField] private float wallJumpHeight = 2f;
    [SerializeField] private float wallPushForce = 10f;
    [SerializeField] private float wallContactMemory = 0.15f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = -60f;
    [SerializeField] private float groundedForce = -2f;

    [Header("Apex")]
    [SerializeField] private float apexThreshold = 2f;
    [SerializeField] private float apexGravityMultiplier = 2f;

    [Header("Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;

    [Header("References")]
    [SerializeField] private PlayerGround playerGround;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerSlide playerSlide;

    private float verticalVelocity;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool wasGrounded;

    private int jumpsUsed;
    private int wallJumpsUsed;
    private CharacterController controller;

private bool IsGrounded =>
    controller.isGrounded ||
    (playerGround != null &&
     playerGround.IsGrounded);

    private Vector3 lastWallNormal;
    private float lastWallContactTime =
        float.NegativeInfinity;

    public float VerticalVelocity => verticalVelocity;

    private void Awake()
    {
        controller =
    GetComponent<CharacterController>();
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (playerGround == null)
            playerGround = GetComponent<PlayerGround>();

        if (playerSlide == null)
            playerSlide = GetComponent<PlayerSlide>();
    }

    private void OnEnable()
    {
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
    }

    private void Update()
    {
        UpdateTimers();
        TryJump();
        UpdateGravity();
    }

    private void UpdateTimers()
    {
        bool isGrounded = IsGrounded;

bool justLanded =
    isGrounded &&
    !wasGrounded &&
    verticalVelocity <= 0f;

if (justLanded)
{
    jumpsUsed = 0;
    wallJumpsUsed = 0;
}

if (isGrounded && verticalVelocity <= 0f)
    coyoteCounter = coyoteTime;
else
    coyoteCounter -= Time.deltaTime;

wasGrounded = isGrounded;
    }

    private void TryJump()
    {
        if (jumpBufferCounter <= 0f)
            return;

        if (CanWallJump())
        {
            PerformWallJump();
            return;
        }

        bool hasGroundJump =
            coyoteCounter > 0f;

        // Если игрок просто упал с края,
        // считаем первый прыжок использованным.
        if (!hasGroundJump && jumpsUsed == 0)
        {
            if (maximumJumps <= 1)
                return;

            jumpsUsed = 1;
        }

        if (jumpsUsed >= maximumJumps)
            return;

        PrepareForJump();

        verticalVelocity = Mathf.Sqrt(
            jumpHeight * -2f * gravity
        );

        jumpsUsed++;
        ConsumeJumpInput();
    }

    private bool CanWallJump()
    {
        bool isGrounded = IsGrounded;

        if (isGrounded)
            return false;

        if (wallJumpsUsed >= maximumWallJumps)
            return false;

        return Time.time - lastWallContactTime <=
               wallContactMemory;
    }

    private void PerformWallJump()
    {
        PrepareForJump();

        verticalVelocity = Mathf.Sqrt(
            wallJumpHeight * -2f * gravity
        );

        if (motor != null)
        {
            motor.SetMomentum(
                lastWallNormal * wallPushForce
            );
        }

        wallJumpsUsed++;
        lastWallContactTime =
            float.NegativeInfinity;

        ConsumeJumpInput();
    }

    private void PrepareForJump()
    {
        if (motor != null)
            motor.PreserveVelocityForJump();

        if (playerSlide != null &&
            playerSlide.IsSliding)
        {
            playerSlide.ExitSlideForJump();
        }
    }

    private void ConsumeJumpInput()
    {
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    private void UpdateGravity()
    {
        bool isGrounded = IsGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedForce;
            return;
        }

        float multiplier = 1f;

        if (verticalVelocity < 0f)
            multiplier = fallMultiplier;
        else if (!jumpAction.IsPressed())
            multiplier = lowJumpMultiplier;
        else if (Mathf.Abs(verticalVelocity) <
                 apexThreshold)
            multiplier = apexGravityMultiplier;

        verticalVelocity +=
            gravity * multiplier * Time.deltaTime;

        verticalVelocity = Mathf.Max(
            verticalVelocity,
            maxFallSpeed
        );
    }

    private void OnControllerColliderHit(
        ControllerColliderHit hit)
    {
        bool isWall =
            Mathf.Abs(hit.normal.y) < 0.3f;

        if (!isWall)
            return;

        lastWallNormal = hit.normal;
        lastWallContactTime = Time.time;
    }
}
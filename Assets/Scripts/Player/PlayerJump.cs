using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction jumpAction;
    private bool jumpConsumed;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

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

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;

    public float VerticalVelocity => verticalVelocity;

    private void Awake()
    {
        if (playerGround == null)
            playerGround = GetComponent<PlayerGround>();
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
    bool isGrounded =
        playerGround != null &&
        playerGround.IsGrounded;

    // Сбрасываем блокировку только когда персонаж
    // действительно стоит или падает на землю.
    if (isGrounded && verticalVelocity <= 0f)
    {
        coyoteCounter = coyoteTime;
        jumpConsumed = false;
    }
    else
    {
        coyoteCounter -= Time.deltaTime;
    }

    if (jumpAction.WasPressedThisFrame())
    {
        jumpBufferCounter = jumpBuffer;
    }
    else
    {
        jumpBufferCounter -= Time.deltaTime;
    }

    coyoteCounter = Mathf.Max(coyoteCounter, 0f);
    jumpBufferCounter = Mathf.Max(jumpBufferCounter, 0f);
}

    private void TryJump()
{
    if (jumpConsumed)
        return;

    if (jumpBufferCounter <= 0f)
        return;

    if (coyoteCounter <= 0f)
        return;

    verticalVelocity = Mathf.Sqrt(
        jumpHeight * -2f * gravity
    );

    jumpConsumed = true;
    jumpBufferCounter = 0f;
    coyoteCounter = 0f;
}

    private void UpdateGravity()
{
    bool isGrounded =
        playerGround != null &&
        playerGround.IsGrounded;

    if (isGrounded && verticalVelocity < 0f)
    {
        verticalVelocity = groundedForce;
        return;
    }

    float gravityMultiplier = 1f;

    // Уже падаем.
    if (verticalVelocity < 0f)
    {
        gravityMultiplier = fallMultiplier;
    }
    // Игрок отпустил прыжок во время подъёма.
    else if (!jumpAction.IsPressed())
    {
        gravityMultiplier = lowJumpMultiplier;
    }
    // Скорость около нуля — вершина прыжка.
    else if (Mathf.Abs(verticalVelocity) < apexThreshold)
    {
        gravityMultiplier = apexGravityMultiplier;
    }

    verticalVelocity +=
        gravity *
        gravityMultiplier *
        Time.deltaTime;

    verticalVelocity = Mathf.Max(
        verticalVelocity,
        maxFallSpeed
    );

    }
}
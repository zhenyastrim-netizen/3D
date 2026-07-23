using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction jumpAction;

    [Header("Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallSpeed = -50f;

    [Header("Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;

    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerGround playerGround;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;

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
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        TryJump();
        ApplyGravity();

        motor.VerticalVelocity = verticalVelocity;
    }

    private void UpdateCoyoteTime()
    {
        if (playerGround.IsGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBuffer;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void TryJump()
    {
        if (jumpBufferCounter <= 0f || coyoteCounter <= 0f)
            return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    private void ApplyGravity()
    {
        if (playerGround.IsGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            return;
        }

        float currentGravity = verticalVelocity < 0f
            ? gravity * fallMultiplier
            : gravity;

        verticalVelocity += currentGravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
    }
}
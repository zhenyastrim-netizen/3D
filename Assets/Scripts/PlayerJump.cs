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

    [Header("Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;

    private CharacterController controller;

    [SerializeField] private PlayerMotor motor;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;

    [SerializeField] private PlayerGround playerGround;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        jumpAction.Enable();
    }

    void OnDisable()
    {
        jumpAction.Disable();
    }

    void Update()
    {
        if (playerGround.IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;
        Debug.Log(playerGround.IsGrounded);
        if (jumpAction.WasPressedThisFrame())
            jumpBufferCounter = jumpBuffer;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        if (playerGround.IsGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        Debug.Log(verticalVelocity);
        motor.VerticalVelocity = verticalVelocity;
    }
}
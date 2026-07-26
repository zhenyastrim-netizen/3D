using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Gravity")]
    

    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerCameraEffects cameraEffects;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private PlayerSlide playerSlide;
    private Vector3 currentVelocity;
    public Vector3 MoveDirection { get; private set; }
    

    void Awake()
    {
        
    }

    void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
    }

    void Update()
    {
        Move();
    }

    private void Move()
{
    if (playerSlide != null && playerSlide.IsSliding)
    {
        currentVelocity = Vector3.zero;
        motor.HorizontalVelocity = Vector3.zero;
        return;
    }

    if (playerDash != null && playerDash.IsDashing)
{
    motor.HorizontalVelocity = Vector3.zero;
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

    bool isSprinting =
        sprintAction.IsPressed() &&
        input.sqrMagnitude > 0.01f;

    float currentSpeed = isSprinting
        ? sprintSpeed
        : moveSpeed;

    if (cameraEffects != null)
        cameraEffects.IsSprinting = isSprinting;

    Vector3 targetVelocity =
        direction * currentSpeed;

    float changeSpeed =
        direction.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

    currentVelocity = Vector3.MoveTowards(
        currentVelocity,
        targetVelocity,
        changeSpeed * Time.deltaTime
    );

    motor.HorizontalVelocity = currentVelocity;
}

}
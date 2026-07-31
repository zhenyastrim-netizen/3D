using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private GroundCheck groundCheck;

    [Header("Momentum")]
    [SerializeField] private float groundMomentumDeceleration = 30f;
    [SerializeField] private float airMomentumDeceleration = 5f;
    [SerializeField] private float momentumStopThreshold = 0.1f;

    private CharacterController controller;
    private Vector3 momentumVelocity;

    public Vector3 HorizontalVelocity { get; set; }
    public Vector3 ExternalVelocity { get; set; }
    public Vector3 KnockbackVelocity { get; set; }

    public Vector3 MomentumVelocity => momentumVelocity;

    public Vector3 Velocity
    {
        get
        {
            Vector3 velocity =
                HorizontalVelocity +
                ExternalVelocity +
                KnockbackVelocity +
                momentumVelocity;

            velocity.y += GetVerticalVelocity();

            return velocity;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (groundCheck == null)
            groundCheck = GetComponent<GroundCheck>();
    }

    private void LateUpdate()
    {
        UpdateMomentum();

        Vector3 finalVelocity =
            HorizontalVelocity +
            ExternalVelocity +
            KnockbackVelocity +
            momentumVelocity;

        if (playerJump != null)
            finalVelocity.y += playerJump.VerticalVelocity;

        controller.Move(
            finalVelocity * Time.deltaTime
        );
    }

    public void SetMomentum(Vector3 velocity)
    {
        velocity.y = 0f;
        momentumVelocity = velocity;
    }

    public void AddMomentum(Vector3 velocity)
    {
        velocity.y = 0f;
        momentumVelocity += velocity;
    }

    public void ClearMomentum()
    {
        momentumVelocity = Vector3.zero;
    }

    private void UpdateMomentum()
    {
        bool isGrounded =
            groundCheck != null &&
            groundCheck.IsGrounded;

        float deceleration = isGrounded
            ? groundMomentumDeceleration
            : airMomentumDeceleration;

        momentumVelocity = Vector3.MoveTowards(
            momentumVelocity,
            Vector3.zero,
            deceleration * Time.deltaTime
        );

        if (momentumVelocity.sqrMagnitude <=
            momentumStopThreshold * momentumStopThreshold)
        {
            momentumVelocity = Vector3.zero;
        }
    }
    public void PreserveVelocityForJump()
{
    Vector3 controlledVelocity =
        HorizontalVelocity +
        ExternalVelocity;

    controlledVelocity.y = 0f;

    Vector3 preservedVelocity =
        momentumVelocity;

    // Сохраняем более быструю скорость,
    // но не складываем их бесконечно.
    if (controlledVelocity.sqrMagnitude >
        preservedVelocity.sqrMagnitude)
    {
        preservedVelocity =
            controlledVelocity;
    }

    momentumVelocity = preservedVelocity;

    HorizontalVelocity = Vector3.zero;
    ExternalVelocity = Vector3.zero;
}

    private float GetVerticalVelocity()
    {
        return playerJump != null
            ? playerJump.VerticalVelocity
            : 0f;
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 24f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Stopping")]
    [SerializeField] private float stoppingDistance = 1.8f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float groundedForce = -2f;
    [SerializeField] private float maxFallSpeed = -50f;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private GroundCheck groundCheck;
    
    [SerializeField] private float attackLungeForce = 4f;
[SerializeField] private EnemyMovement movement;

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    

    public Vector3 Velocity =>
        horizontalVelocity + Vector3.up * verticalVelocity;

    public bool IsMoving =>
        horizontalVelocity.sqrMagnitude > 0.05f;

    private void Awake()
{
    
    controller = GetComponent<CharacterController>();

    if (brain == null)
        brain = GetComponent<EnemyBrain>();

    if (groundCheck == null)
        groundCheck = GetComponent<GroundCheck>();
}

    private void Update()
    {
        if (brain == null)
            return;

        if (brain.CurrentState == EnemyBrain.EnemyState.Dead)
        {
            Stop();
            ApplyGravity();
            MoveController();
            return;
        }

        UpdateHorizontalMovement();
        ApplyGravity();
        MoveController();
    }
    public void AddImpulse(Vector3 impulse)
{
    horizontalVelocity += impulse;
    horizontalVelocity.y = 0f;
}

    private void UpdateHorizontalMovement()
{
    if (brain.Target == null)
    {
        Decelerate();
        return;
    }

    Vector3 toTarget =
        brain.Target.position - transform.position;

    // Двигаемся только по горизонтальной плоскости.
    toTarget.y = 0f;

    float horizontalDistance = toTarget.magnitude;

    if (brain.CurrentState == EnemyBrain.EnemyState.Attack)
    {
        Decelerate();

        if (toTarget.sqrMagnitude > 0.001f)
            RotateTowards(toTarget.normalized);

        return;
    }

    if (brain.CurrentState != EnemyBrain.EnemyState.Chase)
    {
        Decelerate();
        return;
    }

    if (horizontalDistance <= stoppingDistance)
    {
        Decelerate();

        if (toTarget.sqrMagnitude > 0.001f)
            RotateTowards(toTarget.normalized);

        return;
    }

    Vector3 direction = toTarget.normalized;
    Vector3 targetVelocity = direction * moveSpeed;

    horizontalVelocity = Vector3.MoveTowards(
        horizontalVelocity,
        targetVelocity,
        acceleration * Time.deltaTime
    );

    horizontalVelocity.y = 0f;

    RotateTowards(direction);
}

private void Decelerate()
{
    horizontalVelocity = Vector3.MoveTowards(
        horizontalVelocity,
        Vector3.zero,
        deceleration * Time.deltaTime
    );

    horizontalVelocity.y = 0f;
}

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1f - Mathf.Exp(-rotationSpeed * Time.deltaTime)
        );
    }

    private void ApplyGravity()
{
    bool isGrounded =
        groundCheck != null &&
        groundCheck.IsGrounded;

    if (isGrounded && verticalVelocity < 0f)
    {
        verticalVelocity = groundedForce;
        return;
    }

    verticalVelocity += gravity * Time.deltaTime;

    verticalVelocity = Mathf.Max(
        verticalVelocity,
        maxFallSpeed
    );
}

    private void MoveController()
{
    Vector3 finalVelocity = new Vector3(
        horizontalVelocity.x,
        verticalVelocity,
        horizontalVelocity.z
    );

    controller.Move(finalVelocity * Time.deltaTime);
}

    public void Stop()
    {
        horizontalVelocity = Vector3.zero;
    }
}
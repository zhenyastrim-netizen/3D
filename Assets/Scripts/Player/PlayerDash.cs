using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction dashAction;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.4f;

    [Header("Momentum")]
    [SerializeField] private float exitMomentumPercent = 0.65f;
    [SerializeField] private float momentumDeceleration = 35f;
    [SerializeField] private float momentumStopThreshold = 0.1f;

    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerMovement playerMovement;

    public bool IsDashing { get; private set; }
    public bool HasMomentum =>
        momentumVelocity.sqrMagnitude >
        momentumStopThreshold * momentumStopThreshold;

    private bool canDash = true;
    private Vector3 momentumVelocity;
    private Coroutine dashCoroutine;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        dashAction.Enable();
    }

    private void OnDisable()
    {
        dashAction.Disable();

        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);

        IsDashing = false;
        momentumVelocity = Vector3.zero;

        if (motor != null)
            motor.ExternalVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (dashAction.WasPressedThisFrame() && canDash)
        {
            dashCoroutine = StartCoroutine(DashRoutine());
        }

        if (!IsDashing)
            UpdateMomentum();
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        IsDashing = true;

        // Новый dash прерывает остаточную инерцию прошлого.
        momentumVelocity = Vector3.zero;

        Vector3 dashDirection =
            playerMovement != null
                ? playerMovement.MoveDirection
                : Vector3.zero;

        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = transform.forward;

        dashDirection.y = 0f;
        dashDirection.Normalize();

        Vector3 dashVelocity =
            dashDirection * dashSpeed;

        float timer = 0f;

        while (timer < dashDuration)
        {
            motor.ExternalVelocity = dashVelocity;

            timer += Time.deltaTime;
            yield return null;
        }

        // Сохраняем часть скорости dash.
        momentumVelocity =
            dashVelocity * exitMomentumPercent;

        motor.ExternalVelocity =
            momentumVelocity;

        IsDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
        dashCoroutine = null;
    }

    private void UpdateMomentum()
    {
        if (motor == null)
            return;

        momentumVelocity = Vector3.MoveTowards(
            momentumVelocity,
            Vector3.zero,
            momentumDeceleration * Time.deltaTime
        );

        if (momentumVelocity.sqrMagnitude <=
            momentumStopThreshold * momentumStopThreshold)
        {
            momentumVelocity = Vector3.zero;
        }

        motor.ExternalVelocity = momentumVelocity;
    }
}
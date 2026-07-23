using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction dashAction;
    [SerializeField] private InputAction moveAction;

    [Header("Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.4f;

    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerMovement MoveDirection;
    [SerializeField] private PlayerMovement playerMovement;
    public bool IsDashing { get; private set; }

    private bool canDash = true;

    private void OnEnable()
    {
        dashAction.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        dashAction.Disable();
        moveAction.Disable();
    }

    private void Update()
    {
        if (dashAction.WasPressedThisFrame() && canDash)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        
        canDash = false;
        IsDashing = true;

        Vector2 input = moveAction.ReadValue<Vector2>();

Vector3 dashDirection = playerMovement.MoveDirection;

if (dashDirection.sqrMagnitude < 0.01f)
    dashDirection = transform.forward;

dashDirection.Normalize();

        float timer = 0f;

        while (timer < dashDuration)
        {
            motor.ExternalVelocity = dashDirection * dashSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        motor.ExternalVelocity = Vector3.zero;

        IsDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
        
    }
}
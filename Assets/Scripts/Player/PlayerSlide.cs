using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction slideAction;

    [Header("Slide")]
    [SerializeField] private float minimumSlideSpeed = 8f;
    [SerializeField] private float slideSpeedMultiplier = 1f;
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private float slideDeceleration = 4f;
    [SerializeField] private float slideCooldown = 0.2f;

    [Header("Character Height")]
    [SerializeField] private float slidingHeight = 1f;

    [Header("Camera")]
    [SerializeField] private float cameraSlideOffset = 0.65f;
    [SerializeField] private float cameraDownSpeed = 18f;
    [SerializeField] private float cameraUpSpeed = 12f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerGround playerGround;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Transform cameraHolder;

    private Vector3 slideVelocity;
    private Vector3 cameraStartLocalPosition;

    private float originalHeight;
    private Vector3 originalCenter;

    private bool canSlide = true;
    private Coroutine slideCoroutine;

    public bool IsSliding { get; private set; }

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerGround == null)
            playerGround = GetComponent<PlayerGround>();

        if (playerDash == null)
            playerDash = GetComponent<PlayerDash>();

        originalHeight = controller.height;
        originalCenter = controller.center;

        if (cameraHolder != null)
            cameraStartLocalPosition = cameraHolder.localPosition;
    }

    private void OnEnable()
    {
        slideAction.Enable();
    }

    private void OnDisable()
    {
        slideAction.Disable();

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        if (IsSliding)
            StopSlide(false);
    }

    private void Update()
    {
        if (slideAction.WasPressedThisFrame())
            TryStartSlide();

        UpdateCameraHeight();
    }

    private void TryStartSlide()
    {
        if (!canSlide || IsSliding)
            return;

        if (playerGround == null || !playerGround.IsGrounded)
            return;

        if (playerDash != null && playerDash.IsDashing)
            return;

        slideCoroutine = StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        canSlide = false;
        IsSliding = true;

        Vector3 currentVelocity =
            motor.HorizontalVelocity +
            motor.ExternalVelocity +
            motor.MomentumVelocity;

        currentVelocity.y = 0f;

        Vector3 slideDirection;

        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            slideDirection = currentVelocity.normalized;
        }
        else if (playerMovement != null &&
                 playerMovement.MoveDirection.sqrMagnitude > 0.01f)
        {
            slideDirection = playerMovement.MoveDirection.normalized;
        }
        else
        {
            slideDirection = transform.forward;
        }

        slideDirection.y = 0f;
        slideDirection.Normalize();

        float startingSpeed = currentVelocity.magnitude;

        startingSpeed = Mathf.Max(
            startingSpeed,
            minimumSlideSpeed
        );

        slideVelocity =
            slideDirection *
            startingSpeed *
            slideSpeedMultiplier;

        // Переносим всю скорость под управление слайда.
        motor.HorizontalVelocity = Vector3.zero;
        motor.ExternalVelocity = Vector3.zero;
        motor.ClearMomentum();

        SetControllerHeight(slidingHeight);

        float timer = 0f;

        while (
            IsSliding &&
            timer < slideDuration &&
            playerGround.IsGrounded
        )
        {
            motor.ExternalVelocity = slideVelocity;

            slideVelocity = Vector3.MoveTowards(
                slideVelocity,
                Vector3.zero,
                slideDeceleration * Time.deltaTime
            );

            if (slideVelocity.sqrMagnitude <= 0.01f)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (IsSliding)
            StopSlide(true);

        slideCoroutine = null;

        yield return new WaitForSeconds(slideCooldown);

        canSlide = true;
    }

    public void ExitSlideForJump()
    {
        if (!IsSliding)
            return;

        StopSlide(true);
    }

    private void StopSlide(bool preserveMomentum)
    {
        if (!IsSliding)
            return;

        if (preserveMomentum)
            motor.SetMomentum(slideVelocity);

        motor.ExternalVelocity = Vector3.zero;

        controller.height = originalHeight;
        controller.center = originalCenter;

        IsSliding = false;
    }

    private void SetControllerHeight(float newHeight)
    {
        float heightDifference =
            originalHeight - newHeight;

        controller.height = newHeight;

        Vector3 newCenter = originalCenter;
        newCenter.y =
            originalCenter.y -
            heightDifference * 0.5f;

        controller.center = newCenter;
    }

    private void UpdateCameraHeight()
    {
        if (cameraHolder == null)
            return;

        Vector3 targetPosition =
            cameraStartLocalPosition;

        if (IsSliding)
            targetPosition.y -= cameraSlideOffset;

        float speed = IsSliding
            ? cameraDownSpeed
            : cameraUpSpeed;

        cameraHolder.localPosition =
            Vector3.MoveTowards(
                cameraHolder.localPosition,
                targetPosition,
                speed * Time.deltaTime
            );
    }
}
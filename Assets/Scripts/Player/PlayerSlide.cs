using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction slideAction;

    [Header("Settings")]
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private float slideDeceleration = 12f;
    [SerializeField] private float slideCooldown = 0.2f;

    [Header("Character Height")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float slidingHeight = 1f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerGround playerGround;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Transform cameraHolder;
    [Header("Camera")]
[SerializeField] private float cameraSlideOffset = 0.65f;
[SerializeField] private float cameraDownSpeed = 18f;
[SerializeField] private float cameraUpSpeed = 12f;
private Vector3 cameraStartLocalPosition;
    private float originalHeight;
private Vector3 originalCenter;

    public bool IsSliding { get; private set; }

    private bool canSlide = true;
    private Vector3 slideDirection;
    private float currentSlideSpeed;

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
    cameraStartLocalPosition = cameraHolder.localPosition;
}
private void UpdateCameraHeight()
{
    Vector3 targetPosition = cameraStartLocalPosition;

    if (IsSliding)
    {
        targetPosition.y -= cameraSlideOffset;
    }

    float speed = IsSliding
        ? cameraDownSpeed
        : cameraUpSpeed;

    cameraHolder.localPosition = Vector3.MoveTowards(
        cameraHolder.localPosition,
        targetPosition,
        speed * Time.deltaTime
    );
}

    private void OnEnable()
    {
        slideAction.Enable();
    }

    private void OnDisable()
    {
        slideAction.Disable();

        if (IsSliding)
            StopSlide();
    }

    private void Update()
{
    if (slideAction.WasPressedThisFrame())
    {
        TryStartSlide();
    }

    UpdateCameraHeight();
}

    private void TryStartSlide()
    {
        if (!canSlide)
            return;

        if (IsSliding)
            return;

        if (!playerGround.IsGrounded)
            return;

        if (playerDash != null && playerDash.IsDashing)
            return;

        StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        canSlide = false;
        IsSliding = true;

        slideDirection = playerMovement.MoveDirection;

        if (slideDirection.sqrMagnitude < 0.01f)
        {
            slideDirection = transform.forward;
        }

        slideDirection.y = 0f;
        slideDirection.Normalize();

        currentSlideSpeed = slideSpeed;

        SetControllerHeight(slidingHeight);

        float timer = 0f;

        while (timer < slideDuration && playerGround.IsGrounded)
        {
            motor.ExternalVelocity = slideDirection * currentSlideSpeed;

            currentSlideSpeed = Mathf.MoveTowards(
                currentSlideSpeed,
                0f,
                slideDeceleration * Time.deltaTime
            );

            timer += Time.deltaTime;

            if (currentSlideSpeed <= 0.1f)
                break;

            yield return null;
        }

        StopSlide();

        yield return new WaitForSeconds(slideCooldown);

        canSlide = true;
    }

    private void StopSlide()
{
    motor.ExternalVelocity = Vector3.zero;

    controller.height = originalHeight;
    controller.center = originalCenter;

    IsSliding = false;
}

    private void SetControllerHeight(float newHeight)
{
    float heightDifference = originalHeight - newHeight;

    controller.height = newHeight;

    Vector3 newCenter = originalCenter;
    newCenter.y = originalCenter.y - heightDifference * 0.5f;

    controller.center = newCenter;
}
}
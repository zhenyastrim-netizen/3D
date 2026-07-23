using UnityEngine;

public class PlayerCameraEffects : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 90f;
    [SerializeField] private float sprintFOV = 100f;
    [SerializeField] private float fovSpeed = 8f;

    public bool IsSprinting { get; set; }

    private void Update()
    {
        float target = IsSprinting ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.MoveTowards(
    playerCamera.fieldOfView,
    target,
    fovSpeed * Time.deltaTime);
    }
}
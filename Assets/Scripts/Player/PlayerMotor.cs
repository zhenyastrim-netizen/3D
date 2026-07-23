using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;

    public Vector3 HorizontalVelocity { get; set; }
    public float VerticalVelocity { get; set; }
    public Vector3 ExternalVelocity { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 velocity =
            HorizontalVelocity +
            Vector3.up * VerticalVelocity +
            ExternalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private PlayerJump playerJump;

    private CharacterController controller;

    public Vector3 HorizontalVelocity { get; set; }
    public Vector3 ExternalVelocity { get; set; }

    public Vector3 Velocity =>
        HorizontalVelocity +
        Vector3.up * GetVerticalVelocity() +
        ExternalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();
    }

    private void Update()
    {
        Vector3 finalVelocity = HorizontalVelocity + ExternalVelocity;

        finalVelocity.y = GetVerticalVelocity();

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private float GetVerticalVelocity()
    {
        return playerJump != null
            ? playerJump.VerticalVelocity
            : 0f;
    }
    

}
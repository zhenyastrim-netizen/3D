using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float horizontalDecay = 14f;
    [SerializeField] private float verticalDecay = 20f;

    [Header("References")]
    [SerializeField] private PlayerMotor motor;

    private Vector3 currentKnockback;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();
    }

    private void Update()
    {
        UpdateKnockback();
    }

    public void ApplyKnockback(Vector3 velocity)
    {
        currentKnockback = velocity;
    }

    public void AddKnockback(Vector3 velocity)
    {
        currentKnockback += velocity;
    }

    private void UpdateKnockback()
    {
        Vector3 horizontal = new Vector3(
            currentKnockback.x,
            0f,
            currentKnockback.z
        );

        horizontal = Vector3.MoveTowards(
            horizontal,
            Vector3.zero,
            horizontalDecay * Time.deltaTime
        );

        float vertical = Mathf.MoveTowards(
            currentKnockback.y,
            0f,
            verticalDecay * Time.deltaTime
        );

        currentKnockback = new Vector3(
            horizontal.x,
            vertical,
            horizontal.z
        );

        if (motor != null)
            motor.KnockbackVelocity = currentKnockback;
    }

    private void OnDisable()
    {
        currentKnockback = Vector3.zero;

        if (motor != null)
            motor.KnockbackVelocity = Vector3.zero;
    }
}
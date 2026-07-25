using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform groundCheck;

    [Header("Settings")]
    [SerializeField] private float groundRadius = 0.3f;

    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded { get; private set; }

    private void Update()
{
    if (groundCheck == null)
    {
        IsGrounded = false;
        return;
    }

    IsGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundRadius,
        groundMask,
        QueryTriggerInteraction.Ignore
    );
}

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundRadius);
    }
#endif
}
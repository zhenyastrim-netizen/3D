using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float checkRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        CheckGround();
    }

    private void CheckGround()
    {
        if (checkPoint == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics.CheckSphere(
            checkPoint.position,
            checkRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (checkPoint == null)
            return;

        Gizmos.color = IsGrounded
            ? Color.green
            : Color.red;

        Gizmos.DrawWireSphere(
            checkPoint.position,
            checkRadius
        );
    }
#endif
}
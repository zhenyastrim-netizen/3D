using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform groundCheck;

    [Header("Settings")]
    [SerializeField] private float groundRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    private readonly Collider[] groundResults =
        new Collider[8];

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        if (groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(
            groundCheck.position,
            groundRadius,
            groundResults,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        IsGrounded = false;

        for (int i = 0; i < count; i++)
        {
            Collider detectedCollider =
                groundResults[i];

            if (detectedCollider == null)
                continue;

            if (detectedCollider.transform.root ==
                transform.root)
            {
                continue;
            }

            IsGrounded = true;
            break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = IsGrounded
            ? Color.green
            : Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundRadius
        );
    }
#endif
}
using System.Collections;
using UnityEngine;

public class FlashGrenade : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float fuseTime = 0.5f;

    [Header("Effect")]
    [SerializeField] private float radius = 6f;
    [SerializeField] private float blindDuration = 1.5f;
    [SerializeField] private float slowDuration = 2f;

    [SerializeField, Range(0f, 1f)]
    private float slowPercent = 0.4f;

    [Header("Layers")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(fuseTime);

        Explode();

        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            playerMask,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            PlayerFlashStatus status =
                hit.GetComponentInParent<PlayerFlashStatus>();

            if (status == null)
                continue;

            Vector3 targetPosition =
                status.transform.position +
                Vector3.up;

            bool blocked = Physics.Linecast(
                transform.position + Vector3.up * 0.2f,
                targetPosition,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

            if (blocked)
                continue;

            status.ApplyFlash(
                blindDuration,
                slowDuration,
                slowPercent
            );

            break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
#endif
}
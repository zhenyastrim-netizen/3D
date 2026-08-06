using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class UncertaintyProjectile : MonoBehaviour
{
    private Rigidbody body;
    private Collider projectileCollider;
    private DamagePart[] damageParts;
    private AttackType attackType;
    private GameObject source;
    private bool initialized;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();

        body.useGravity = false;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
        projectileCollider.isTrigger = true;
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float lifetime,
        DamagePart[] parts,
        AttackType originalAttackType,
        GameObject projectileSource)
    {
        source = projectileSource;
        damageParts = parts != null
            ? (DamagePart[])parts.Clone()
            : new DamagePart[0];
        attackType = originalAttackType;
        initialized = true;

        direction.Normalize();
        transform.forward = direction;
        body.linearVelocity = direction * speed;

        IgnoreSourceCollisions();
        Destroy(gameObject, Mathf.Max(0.1f, lifetime));
    }

    private void IgnoreSourceCollisions()
    {
        if (source == null || projectileCollider == null)
            return;

        Collider[] sourceColliders =
            source.GetComponentsInChildren<Collider>();

        foreach (Collider sourceCollider in sourceColliders)
        {
            Physics.IgnoreCollision(
                projectileCollider,
                sourceCollider
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized)
            return;

        if (source != null &&
            other.transform.root == source.transform.root)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            Destroy(gameObject);
            return;
        }

        damageable.TakeDamage(
            new DamageInfo(
                damageParts,
                attackType,
                false,
                source,
                true
            )
        );

        Destroy(gameObject);
    }
}

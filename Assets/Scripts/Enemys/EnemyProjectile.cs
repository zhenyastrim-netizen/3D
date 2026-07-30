using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody body;
    private DamagePart[] damageParts;
    private GameObject source;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float lifetime,
        DamagePart[] parts,
        GameObject projectileSource)
    {
        source = projectileSource;
        damageParts = parts;

        IgnoreSourceCollisions();

        body.linearVelocity =
            direction.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    private void IgnoreSourceCollisions()
    {
        if (source == null)
            return;

        Collider projectileCollider =
            GetComponent<Collider>();

        Collider[] sourceColliders =
            source.GetComponentsInChildren<Collider>();

        foreach (Collider sourceCollider
                 in sourceColliders)
        {
            Physics.IgnoreCollision(
                projectileCollider,
                sourceCollider
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (source != null &&
            collision.transform.root ==
            source.transform.root)
        {
            return;
        }

        IDamageable damageable =
            collision.collider
                .GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            DamageInfo damageInfo = new DamageInfo(
                damageParts,
                AttackType.Ranged,
                false,
                source
            );

            damageable.TakeDamage(damageInfo);
        }

        Destroy(gameObject);
    }
}
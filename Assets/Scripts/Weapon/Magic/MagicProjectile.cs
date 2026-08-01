using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class MagicProjectile : MonoBehaviour
{
    private Rigidbody projectileRigidbody;

    private DamageInfo damageInfo;
    private GameObject source;

    private bool initialized;

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();

        projectileRigidbody.useGravity = false;
        projectileRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void Initialize(
        SpellData spell,
        Vector3 direction,
        DamageInfo newDamageInfo,
        GameObject newSource)
    {
        damageInfo = newDamageInfo;
        source = newSource;
        initialized = true;

        direction.Normalize();
        transform.forward = direction;

        projectileRigidbody.linearVelocity =
            direction * spell.ProjectileSpeed;

        float lifetime =
            spell.Range / spell.ProjectileSpeed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized)
            return;

        // Не попадаем в самого игрока
        if (source != null &&
            other.transform.root == source.transform.root)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(damageInfo);

        Destroy(gameObject);
    }
}
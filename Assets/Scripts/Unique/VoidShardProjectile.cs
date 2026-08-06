using UnityEngine;

public class VoidShardProjectile : MonoBehaviour
{
    private Transform target;
    private PlayerDamageCalculator damageCalculator;
    private GameObject damageSource;
    private DamagePart[] damageParts;
    private float speed;
    private float turnSpeedRadians;
    private float hitRadius;
    private float remainingLifetime;
    private bool isLaunched;

    public void StoreDamage(
        DamagePart[] baseDamageParts,
        float damageMultiplier)
    {
        if (baseDamageParts == null)
        {
            damageParts = new DamagePart[0];
            return;
        }

        damageParts = new DamagePart[baseDamageParts.Length];

        for (int i = 0; i < baseDamageParts.Length; i++)
        {
            DamagePart part = baseDamageParts[i];
            part.damage *= damageMultiplier;
            part.buildup *= damageMultiplier;
            damageParts[i] = part;
        }
    }

    public void Launch(
        Transform newTarget,
        PlayerDamageCalculator newDamageCalculator,
        GameObject newDamageSource,
        float newSpeed,
        float newTurnSpeedDegrees,
        float newHitRadius,
        float lifetime)
    {
        target = newTarget;
        damageCalculator = newDamageCalculator;
        damageSource = newDamageSource;
        speed = Mathf.Max(0.1f, newSpeed);
        turnSpeedRadians = Mathf.Max(1f, newTurnSpeedDegrees) *
                           Mathf.Deg2Rad;
        hitRadius = Mathf.Max(0.01f, newHitRadius);
        remainingLifetime = Mathf.Max(0.1f, lifetime);
        isLaunched = target != null;

        if (!isLaunched)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = GetTargetPoint() - transform.position;

        if (direction.sqrMagnitude > 0.0001f)
            transform.forward = direction.normalized;
    }

    private void Update()
    {
        if (!isLaunched)
            return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPoint = GetTargetPoint();
        Vector3 toTarget = targetPoint - transform.position;

        float frameTravelDistance = speed * Time.deltaTime;
        float captureRadius = Mathf.Max(
            hitRadius,
            frameTravelDistance
        );

        if (toTarget.sqrMagnitude <= captureRadius * captureRadius)
        {
            HitTarget();
            return;
        }

        Vector3 desiredDirection = toTarget.normalized;
        Vector3 currentDirection = transform.forward;

        if (currentDirection.sqrMagnitude <= 0.0001f)
            currentDirection = desiredDirection;

        transform.forward = Vector3.RotateTowards(
            currentDirection,
            desiredDirection,
            turnSpeedRadians * Time.deltaTime,
            0f
        );

        transform.position +=
            transform.forward * speed * Time.deltaTime;
    }

    private Vector3 GetTargetPoint()
    {
        Collider targetCollider =
            target.GetComponentInChildren<Collider>();

        return targetCollider != null
            ? targetCollider.bounds.center
            : target.position;
    }

    private void HitTarget()
    {
        IDamageable damageable =
            target.GetComponentInParent<IDamageable>();

        if (damageable == null)
            damageable = target.GetComponentInChildren<IDamageable>();

        if (damageable != null &&
            damageParts != null &&
            damageParts.Length > 0)
        {
            DamageInfo damageInfo = damageCalculator != null
                ? damageCalculator.CreateDamage(
                    damageParts,
                    AttackType.Ranged,
                    damageSource,
                    true
                )
                : new DamageInfo(
                    damageParts,
                    AttackType.Ranged,
                    false,
                    damageSource,
                    true
                );

            damageable.TakeDamage(damageInfo);
        }

        Destroy(gameObject);
    }
}

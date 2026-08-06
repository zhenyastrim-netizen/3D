using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class RottenHawkProjectile : MonoBehaviour
{
    private const int TargetBufferSize = 32;

    private readonly Collider[] targetBuffer =
        new Collider[TargetBufferSize];

    private Rigidbody body;
    private SphereCollider projectileCollider;
    private PlayerDamageCalculator damageCalculator;
    private DamagePart[] damageParts;
    private GameObject source;
    private Transform target;
    private Collider targetCollider;
    private LayerMask targetMask;
    private LayerMask obstacleMask;
    private float speed;
    private float remainingLifetime;
    private float acquisitionDistance;
    private float acquisitionAngle;
    private float turnSpeedRadians;
    private float remainingHomingTime;
    private float reacquireInterval;
    private float nextTargetSearchTime;
    private bool requireLineOfSight;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<SphereCollider>();

        body.useGravity = false;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        projectileCollider.isTrigger = true;
    }

    public void Initialize(
        Vector3 direction,
        float newSpeed,
        float lifetime,
        DamagePart[] parts,
        PlayerDamageCalculator newDamageCalculator,
        GameObject projectileSource,
        LayerMask newTargetMask,
        float newAcquisitionDistance,
        float newAcquisitionAngle,
        float turnSpeedDegrees,
        float newHomingDuration,
        float newReacquireInterval,
        bool newRequireLineOfSight,
        LayerMask newObstacleMask)
    {
        source = projectileSource;
        damageCalculator = newDamageCalculator;
        damageParts = parts != null
            ? (DamagePart[])parts.Clone()
            : new DamagePart[0];
        targetMask = newTargetMask;
        obstacleMask = newObstacleMask;
        speed = Mathf.Max(0.1f, newSpeed);
        remainingLifetime = Mathf.Max(0.1f, lifetime);
        acquisitionDistance = Mathf.Max(
            0.1f,
            newAcquisitionDistance
        );
        acquisitionAngle = Mathf.Clamp(
            newAcquisitionAngle,
            0f,
            90f
        );
        turnSpeedRadians = Mathf.Max(0f, turnSpeedDegrees) *
                           Mathf.Deg2Rad;
        remainingHomingTime = Mathf.Max(0f, newHomingDuration);
        reacquireInterval = Mathf.Max(
            0.02f,
            newReacquireInterval
        );
        requireLineOfSight = newRequireLineOfSight;

        Vector3 initialDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : transform.forward;

        transform.forward = initialDirection;
        body.linearVelocity = initialDirection * speed;

        IgnoreSourceCollisions();
        AcquireTarget();
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized || hasHit)
            return;

        remainingLifetime -= Time.fixedDeltaTime;
        remainingHomingTime = Mathf.Max(
            0f,
            remainingHomingTime - Time.fixedDeltaTime
        );

        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (remainingHomingTime > 0f &&
            !IsTargetValid() &&
            Time.time >= nextTargetSearchTime)
        {
            AcquireTarget();
        }

        Vector3 currentDirection = body.linearVelocity.sqrMagnitude > 0.0001f
            ? body.linearVelocity.normalized
            : transform.forward;

        if (remainingHomingTime > 0f &&
            IsTargetValid() &&
            turnSpeedRadians > 0f)
        {
            Vector3 toTarget = GetTargetPoint() - body.position;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                currentDirection = Vector3.RotateTowards(
                    currentDirection,
                    toTarget.normalized,
                    turnSpeedRadians * Time.fixedDeltaTime,
                    0f
                );
            }
        }

        body.linearVelocity = currentDirection * speed;

        if (currentDirection.sqrMagnitude > 0.0001f)
            transform.forward = currentDirection;
    }

    private void AcquireTarget()
    {
        nextTargetSearchTime = Time.time + reacquireInterval;
        target = null;
        targetCollider = null;

        Vector3 forward = body != null &&
                          body.linearVelocity.sqrMagnitude > 0.0001f
            ? body.linearVelocity.normalized
            : transform.forward;

        int targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            acquisitionDistance,
            targetBuffer,
            targetMask,
            QueryTriggerInteraction.Ignore
        );

        float bestScore = float.MaxValue;

        for (int i = 0; i < targetCount; i++)
        {
            Collider candidate = targetBuffer[i];

            if (!TryGetLivingTarget(
                    candidate,
                    out Transform candidateTarget))
            {
                continue;
            }

            Vector3 targetPoint = candidate.bounds.center;
            Vector3 toTarget = targetPoint - transform.position;
            float distance = toTarget.magnitude;

            if (distance <= 0.001f)
                continue;

            float angle = Vector3.Angle(
                forward,
                toTarget / distance
            );

            if (angle > acquisitionAngle)
                continue;

            if (requireLineOfSight &&
                !HasLineOfSight(candidateTarget, targetPoint))
            {
                continue;
            }

            float angleScore = acquisitionAngle > 0.001f
                ? angle / acquisitionAngle
                : 0f;
            float distanceScore = distance / acquisitionDistance;
            float score = angleScore * 0.75f +
                          distanceScore * 0.25f;

            if (score >= bestScore)
                continue;

            bestScore = score;
            target = candidateTarget;
            targetCollider = candidate;
        }

        for (int i = 0; i < targetCount; i++)
            targetBuffer[i] = null;
    }

    private bool TryGetLivingTarget(
        Collider candidate,
        out Transform candidateTarget)
    {
        candidateTarget = null;

        if (candidate == null)
            return false;

        if (source != null &&
            candidate.transform.root == source.transform.root)
        {
            return false;
        }

        IDamageable damageable =
            candidate.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return false;

        Component damageableComponent = damageable as Component;

        if (damageableComponent == null)
            return false;

        EnemyHealth enemyHealth =
            damageableComponent.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        candidateTarget = damageableComponent.transform;
        return true;
    }

    private bool IsTargetValid()
    {
        if (target == null)
            return false;

        EnemyHealth enemyHealth =
            target.GetComponentInParent<EnemyHealth>();

        return enemyHealth == null || !enemyHealth.IsDead;
    }

    private Vector3 GetTargetPoint()
    {
        return targetCollider != null
            ? targetCollider.bounds.center
            : target.position;
    }

    private bool HasLineOfSight(
        Transform candidateTarget,
        Vector3 targetPoint)
    {
        Vector3 toTarget = targetPoint - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
            return true;

        if (!Physics.Raycast(
                transform.position,
                toTarget / distance,
                out RaycastHit hit,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.collider.transform.root ==
               candidateTarget.root;
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
        if (!initialized || hasHit)
            return;

        if (source != null &&
            other.transform.root == source.transform.root)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null && other.isTrigger)
            return;

        hasHit = true;

        if (damageable != null && damageParts.Length > 0)
        {
            DamageInfo damageInfo = damageCalculator != null
                ? damageCalculator.CreateDamage(
                    damageParts,
                    AttackType.Ranged,
                    source
                )
                : new DamageInfo(
                    damageParts,
                    AttackType.Ranged,
                    false,
                    source
                );

            damageable.TakeDamage(damageInfo);
        }

        Destroy(gameObject);
    }

    public static RottenHawkProjectile CreateFallback(
        Vector3 position,
        Quaternion rotation,
        float scale)
    {
        GameObject root = new GameObject(
            "Rotten Hawk Projectile (Runtime)"
        );

        root.transform.SetPositionAndRotation(position, rotation);
        root.transform.localScale = Vector3.one *
                                    Mathf.Max(0.01f, scale);

        Rigidbody rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;

        SphereCollider sphereCollider =
            root.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.45f;
        sphereCollider.isTrigger = true;

        RottenHawkProjectile projectile =
            root.AddComponent<RottenHawkProjectile>();

        CreateFallbackPart(
            root.transform,
            "Body",
            new Vector3(0f, 0f, 0.12f),
            new Vector3(0.18f, 0.08f, 0.6f),
            Quaternion.identity
        );
        CreateFallbackPart(
            root.transform,
            "Left Wing",
            new Vector3(-0.28f, 0f, 0f),
            new Vector3(0.55f, 0.055f, 0.22f),
            Quaternion.Euler(0f, -18f, -10f)
        );
        CreateFallbackPart(
            root.transform,
            "Right Wing",
            new Vector3(0.28f, 0f, 0f),
            new Vector3(0.55f, 0.055f, 0.22f),
            Quaternion.Euler(0f, 18f, 10f)
        );

        return projectile;
    }

    private static void CreateFallbackPart(
        Transform parent,
        string partName,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion localRotation)
    {
        GameObject part = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;

        Collider partCollider = part.GetComponent<Collider>();

        if (partCollider != null)
        {
            partCollider.enabled = false;
            Destroy(partCollider);
        }

        Renderer renderer = part.GetComponent<Renderer>();

        if (renderer != null)
            renderer.material.color = new Color(0.12f, 0.22f, 0.08f);
    }
}

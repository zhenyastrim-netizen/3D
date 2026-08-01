using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyLeapSlam : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float minimumDistance = 6f;
    [SerializeField] private float maximumDistance = 14f;
    [SerializeField] private float cooldown = 4f;

    [Header("Leap")]
    [SerializeField] private float windupTime = 0.6f;
    [SerializeField] private float leapDuration = 0.65f;
    [SerializeField] private float leapHeight = 4f;
    [SerializeField] private float landingDistance = 1.5f;

    [Header("Landing Attack")]
    [SerializeField] private float damage = 40f;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private LayerMask targetMask;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float upwardForce = 6f;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyCombat meleeCombat;

    private CharacterController controller;

    private bool isLeaping;
    private bool movementWasEnabled;
    private bool combatWasEnabled;
    private float nextLeapTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();

        if (meleeCombat == null)
            meleeCombat = GetComponent<EnemyCombat>();
    }

    private void Update()
    {
        if (isLeaping || Time.time < nextLeapTime)
            return;

        if (enemyHealth != null && enemyHealth.IsDead)
            return;

        if (brain == null || brain.Target == null)
            return;

        if (brain.CurrentState != EnemyBrain.EnemyState.Chase)
            return;

        float distance = GetDistanceToTarget();

        if (distance >= minimumDistance &&
            distance <= maximumDistance)
        {
            StartCoroutine(LeapRoutine());
        }
    }

    private IEnumerator LeapRoutine()
    {
        isLeaping = true;

        movementWasEnabled =
            movement != null && movement.enabled;

        combatWasEnabled =
            meleeCombat != null && meleeCombat.enabled;

        if (movement != null)
        {
            movement.Stop();
            movement.enabled = false;
        }

        if (meleeCombat != null)
            meleeCombat.enabled = false;

        yield return new WaitForSeconds(windupTime);

        if (!CanContinue())
        {
            FinishLeap();
            yield break;
        }

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = brain.Target.position;

        targetPosition.y = startPosition.y;

        Vector3 direction =
            targetPosition - startPosition;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            FinishLeap();
            yield break;
        }

        direction.Normalize();

        Vector3 landingPosition =
            targetPosition -
            direction * landingDistance;

        transform.rotation =
            Quaternion.LookRotation(direction);

        float elapsed = 0f;

        while (elapsed < leapDuration)
        {
            if (!CanContinue())
            {
                FinishLeap();
                yield break;
            }

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / leapDuration
            );

            Vector3 horizontalPosition =
                Vector3.Lerp(
                    startPosition,
                    landingPosition,
                    progress
                );

            float verticalOffset =
                Mathf.Sin(progress * Mathf.PI) *
                leapHeight;

            Vector3 desiredPosition =
                horizontalPosition +
                Vector3.up * verticalOffset;

            controller.Move(
                desiredPosition - transform.position
            );

            yield return null;
        }

        PerformLandingHit();
        FinishLeap();
    }

    private void PerformLandingHit()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            damageRadius,
            targetMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            DamagePart[] parts =
            {
                new DamagePart(
                    DamageType.Kinetic,
                    damage
                )
            };

            DamageInfo damageInfo =
                new DamageInfo(
                    parts,
                    AttackType.Melee,
                    false,
                    gameObject
                );

            damageable.TakeDamage(damageInfo);

            PlayerKnockback knockback =
                hit.GetComponentInParent<PlayerKnockback>();

            if (knockback != null)
            {
                Vector3 direction =
                    hit.transform.position -
                    transform.position;

                direction.y = 0f;

                if (direction.sqrMagnitude < 0.001f)
                    direction = transform.forward;

                direction.Normalize();

                knockback.ApplyKnockback(
                    direction * knockbackForce +
                    Vector3.up * upwardForce
                );
            }

            break;
        }
    }

    private bool CanContinue()
    {
        return brain != null &&
               brain.Target != null &&
               (enemyHealth == null || !enemyHealth.IsDead);
    }

    private float GetDistanceToTarget()
    {
        Vector3 difference =
            brain.Target.position - transform.position;

        difference.y = 0f;

        return difference.magnitude;
    }

    private void FinishLeap()
    {
        bool isAlive =
            enemyHealth == null || !enemyHealth.IsDead;

        if (isAlive)
        {
            if (movement != null)
            {
                movement.enabled = movementWasEnabled;
                movement.Stop();
            }

            if (meleeCombat != null)
                meleeCombat.enabled = combatWasEnabled;
        }

        nextLeapTime = Time.time + cooldown;
        isLeaping = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (isLeaping && gameObject.activeInHierarchy)
            FinishLeap();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }
#endif
}
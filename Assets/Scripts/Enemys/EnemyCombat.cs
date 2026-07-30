using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackRange = 2.3f;
    [SerializeField] private float attackRadius = 0.65f;

    [Header("Timing")]
    [SerializeField] private float windupTime = 0.35f;
    [SerializeField] private float recoveryTime = 0.25f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Layers")]
    [SerializeField] private LayerMask targetMask;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Transform attackPoint;
    [Header("Knockback")]
[SerializeField] private float knockbackForce = 12f;
[SerializeField] private float knockbackUpwardForce = 3f;
[Header("Telegraph")]
[SerializeField] private Renderer enemyRenderer;
[SerializeField] private Color normalColor = Color.white;
[SerializeField] private Color warningColor = Color.red;
[SerializeField] private float attackLungeForce = 4f;
[SerializeField] private EnemyMovement movement;
private Material enemyMaterial;

    private bool isAttacking;
    private float nextAttackTime;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        if (movement == null)
    movement = GetComponent<EnemyMovement>();
        if (enemyRenderer != null)
{
    enemyMaterial = enemyRenderer.material;
    normalColor = enemyMaterial.color;
}
        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (brain == null)
            return;

        if (enemyHealth != null && enemyHealth.IsDead)
            return;

        if (brain.CurrentState != EnemyBrain.EnemyState.Attack)
            return;

        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
{
    isAttacking = true;

    SetWarningVisual(true);

    yield return new WaitForSeconds(windupTime);

    SetWarningVisual(false);

    if (brain == null ||
        brain.CurrentState != EnemyBrain.EnemyState.Attack)
    {
        isAttacking = false;
        yield break;
    }

    if (CanHitTarget())
    {
        if (movement != null)
{
    movement.AddImpulse(
        transform.forward * attackLungeForce
    );
}
        PerformHit();
    }

    yield return new WaitForSeconds(recoveryTime);

    nextAttackTime = Time.time + attackCooldown;
    isAttacking = false;
}
    private void SetWarningVisual(bool warning)
{
    if (enemyMaterial == null)
        return;

    enemyMaterial.color =
        warning ? warningColor : normalColor;
}

    private bool CanHitTarget()
    {
        if (brain == null || brain.Target == null)
            return false;

        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        Vector3 difference =
            brain.Target.position - transform.position;

        difference.y = 0f;

        return difference.sqrMagnitude <=
               attackRange * attackRange;
    }

    private void PerformHit()
    {
        Vector3 center = attackPoint != null
            ? attackPoint.position
            : transform.position + transform.forward * attackRange;

        Collider[] hits = Physics.OverlapSphere(
            center,
            attackRadius,
            targetMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
{
    IDamageable damageable =
        hits[i].GetComponentInParent<IDamageable>();

    if (damageable == null)
        continue;

    DamagePart[] damageParts =
    {
        new DamagePart(
            DamageType.Kinetic,
            damage
        )
    };

    DamageInfo damageInfo = new DamageInfo(
        damageParts,
        AttackType.Melee,
        false,
        gameObject
    );

    damageable.TakeDamage(damageInfo);

    PlayerKnockback knockback =
        hits[i].GetComponentInParent<PlayerKnockback>();

    if (knockback != null)
    {
        Vector3 direction =
            hits[i].transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        direction.Normalize();

        Vector3 velocity =
            direction * knockbackForce +
            Vector3.up * knockbackUpwardForce;

        knockback.ApplyKnockback(velocity);
    }

    Debug.Log(
        $"{gameObject.name} нанёс {damage} урона"
    );

    break;
}
    }

    private void OnDisable()
{
    StopAllCoroutines();
    isAttacking = false;
    SetWarningVisual(false);
}

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = attackPoint != null
            ? attackPoint.position
            : transform.position + transform.forward * attackRange;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
#endif
}
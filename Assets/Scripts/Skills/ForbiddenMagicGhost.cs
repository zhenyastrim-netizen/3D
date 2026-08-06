using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
public class ForbiddenMagicGhost : MonoBehaviour
{
    private GameObject player;
    private MagicSkillRuntime owner;
    private EnemyHealth enemyHealth;

    private float moveSpeed;
    private float attackDamage;
    private float attackInterval;
    private float attackRange;
    private float nextAttackTime;
    private bool initialized;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public void Initialize(
        GameObject targetPlayer,
        MagicSkillRuntime newOwner,
        float newMoveSpeed,
        float newAttackDamage,
        float newAttackInterval,
        float newAttackRange)
    {
        player = targetPlayer;
        owner = newOwner;
        moveSpeed = Mathf.Max(0f, newMoveSpeed);
        attackDamage = Mathf.Max(0f, newAttackDamage);
        attackInterval = Mathf.Max(0.05f, newAttackInterval);
        attackRange = Mathf.Max(0f, newAttackRange);
        initialized = true;

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        enemyHealth.OnDied -= HandleDeath;
        enemyHealth.OnDied += HandleDeath;
    }

    private void Update()
    {
        if (!initialized || player == null || enemyHealth.IsDead)
            return;

        Vector3 targetPosition = player.transform.position;
        Vector3 offset = targetPosition - transform.position;
        float distance = offset.magnitude;

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (offset.sqrMagnitude > 0.001f)
                transform.forward = offset.normalized;

            return;
        }

        if (Time.time < nextAttackTime)
            return;

        AttackPlayer();
        nextAttackTime = Time.time + attackInterval;
    }

    private void AttackPlayer()
    {
        PlayerHealth playerHealth =
            player.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        DamagePart[] parts =
        {
            new DamagePart(DamageType.Cursed, attackDamage)
        };

        playerHealth.TakeDamage(
            new DamageInfo(
                parts,
                AttackType.Magic,
                false,
                gameObject,
                true
            )
        );
    }

    private void HandleDeath(EnemyHealth health)
    {
        enemyHealth.OnDied -= HandleDeath;
        owner?.NotifyGhostKilled(this);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.OnDied -= HandleDeath;
    }
}

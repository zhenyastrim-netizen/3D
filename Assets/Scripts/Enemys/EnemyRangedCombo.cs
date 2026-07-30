using System.Collections;
using UnityEngine;

public class EnemyRangedCombat : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private DamagePart[] damageParts =
    {
        new DamagePart(
            DamageType.Kinetic,
            15f
        )
    };

    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 6f;

    [Header("Timing")]
    [SerializeField] private float windupTime = 0.4f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Aim")]
    [SerializeField] private float targetHeight = 1f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyHealth enemyHealth;

    private bool isAttacking;
    private float nextAttackTime;
    private float statusDamageMultiplier = 1f;

    private void Awake()
    {
        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (brain == null ||
            brain.Target == null ||
            isAttacking)
        {
            return;
        }

        if (enemyHealth != null &&
            enemyHealth.IsDead)
        {
            return;
        }

        if (brain.CurrentState !=
            EnemyBrain.EnemyState.Attack)
        {
            return;
        }

        if (Time.time < nextAttackTime)
            return;

        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(
            windupTime
        );

        if (brain == null ||
            brain.Target == null ||
            brain.CurrentState !=
            EnemyBrain.EnemyState.Attack)
        {
            isAttacking = false;
            yield break;
        }

        Shoot();

        nextAttackTime =
            Time.time + attackCooldown;

        isAttacking = false;
    }

    private void Shoot()
    {
        Debug.Log(
    $"{gameObject.name} пытается выстрелить",
    this
);
        if (projectilePrefab == null)
{
    Debug.LogError(
        "EnemyRangedCombat: не назначен Projectile Prefab!",
        this
    );

    return;
}

if (firePoint == null)
{
    Debug.LogError(
        "EnemyRangedCombat: не назначен Fire Point!",
        this
    );

    return;
}

        Vector3 targetPosition =
            brain.Target.position +
            Vector3.up * targetHeight;

        Vector3 direction =
            targetPosition - firePoint.position;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );
        Debug.Log(
    $"Снаряд создан: {projectile.name}",
    projectile
);

        DamagePart[] finalParts =
            new DamagePart[damageParts.Length];

        for (int i = 0; i < damageParts.Length; i++)
        {
            DamagePart part = damageParts[i];

            part.damage *= statusDamageMultiplier;
            finalParts[i] = part;
        }

        projectile.Initialize(
            direction,
            projectileSpeed,
            projectileLifetime,
            finalParts,
            gameObject
        );

        muzzleFlash?.Play();
    }

    public void SetStatusDamageMultiplier(
        float multiplier)
    {
        statusDamageMultiplier =
            Mathf.Clamp(multiplier, 0.1f, 1f);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
    }
}
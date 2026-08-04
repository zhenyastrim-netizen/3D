using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyCombat meleeCombat;
    [SerializeField] private EnemyApproachDash approachDash;
    [SerializeField] private EnemyLeapSlam leapSlam;

    [Header("Decision")]
    [SerializeField, Min(0.1f)] private float closeRange = 3f;
    [SerializeField, Min(0f)] private float timeBetweenActions = 0.55f;
    [SerializeField, Range(0f, 1f)] private float grenadeChance = 0.35f;
    [SerializeField, Range(0f, 1f)] private float phaseTwoLeapChance = 0.45f;
    [SerializeField, Min(1)] private int closeComboHits = 3;
    [SerializeField, Min(1)] private int dashComboHits = 2;

    [Header("Bullet Grenade")]
    [SerializeField] private BossBulletGrenade bulletGrenadePrefab;
    [SerializeField] private Transform grenadeThrowPoint;
    [SerializeField, Min(0f)] private float grenadeWindup = 0.8f;
    [SerializeField, Min(0f)] private float grenadeRecovery = 0.45f;
    [SerializeField, Min(0f)] private float grenadeCooldown = 4f;
    [SerializeField, Min(0.1f)] private float grenadeForwardSpeed = 9f;
    [SerializeField, Min(0f)] private float grenadeUpwardSpeed = 5f;
    [SerializeField, Range(0f, 30f)] private float doubleGrenadeSpread = 10f;

    [Header("25% Summon")]
    [SerializeField] private GameObject[] summonedEnemyPrefabs;
    [SerializeField] private Transform[] summonPoints;
    [SerializeField, Min(0f)] private float summonWindup = 1.2f;
    [SerializeField, Min(0f)] private float summonRecovery = 0.6f;

    private bool isPerformingAction;
    private bool summonedAtLowHealth;
    private bool movementWasEnabled;
    private bool movementPaused;
    private float nextActionTime;
    private float nextGrenadeTime;

    private bool IsPhaseTwo => HealthRatio <= 0.5f;
    private bool IsPhaseThree => HealthRatio <= 0.25f;
    private float HealthRatio => health == null || health.MaxHealth <= 0f
        ? 1f
        : health.CurrentHealth / health.MaxHealth;

    private void Awake()
    {
        if (brain == null) brain = GetComponent<EnemyBrain>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (movement == null) movement = GetComponent<EnemyMovement>();
        if (meleeCombat == null) meleeCombat = GetComponent<EnemyCombat>();
        if (approachDash == null) approachDash = GetComponent<EnemyApproachDash>();
        if (leapSlam == null) leapSlam = GetComponent<EnemyLeapSlam>();

        meleeCombat?.SetAutomaticControl(false);
        approachDash?.SetAutomaticControl(false);
        leapSlam?.SetAutomaticControl(false);
    }

    private void Update()
    {
        if (isPerformingAction || Time.time < nextActionTime)
            return;

        if (health != null && health.IsDead)
            return;

        if (brain == null || brain.Target == null ||
            brain.CurrentState == EnemyBrain.EnemyState.Idle)
        {
            return;
        }

        if (IsPhaseThree && !summonedAtLowHealth)
        {
            summonedAtLowHealth = true;
            StartBossAction(SummonRoutine());
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance <= closeRange)
        {
            if (CanThrowGrenade() && Random.value < grenadeChance)
                StartBossAction(ThrowGrenadesRoutine());
            else
                StartBossAction(MeleeRoutine(closeComboHits));

            return;
        }

        if (IsPhaseTwo && leapSlam != null && leapSlam.IsReady &&
            Random.value < phaseTwoLeapChance)
        {
            StartBossAction(LeapRoutine());
        }
        else if (CanThrowGrenade() && Random.value < grenadeChance)
        {
            StartBossAction(ThrowGrenadesRoutine());
        }
        else
        {
            StartBossAction(DashComboRoutine());
        }
    }

    private void StartBossAction(IEnumerator routine)
    {
        isPerformingAction = true;
        StartCoroutine(routine);
    }

    private IEnumerator MeleeRoutine(int hitCount)
    {
        if (meleeCombat == null || !meleeCombat.TryStartAttack(hitCount))
        {
            FinishAction(0.1f);
            yield break;
        }

        while (meleeCombat.IsAttacking && IsAlive())
            yield return null;

        FinishAction(timeBetweenActions);
    }

    private IEnumerator DashComboRoutine()
    {
        if (approachDash == null || !approachDash.TryStartDash())
        {
            if (CanThrowGrenade())
                yield return ThrowGrenadesRoutine();
            else
                FinishAction(0.15f);

            yield break;
        }

        while (approachDash.IsDashing && IsAlive())
            yield return null;

        if (!IsAlive())
        {
            FinishAction(0f);
            yield break;
        }

        yield return new WaitForSeconds(0.08f);

        if (meleeCombat != null && meleeCombat.TryStartAttack(dashComboHits))
        {
            while (meleeCombat.IsAttacking && IsAlive())
                yield return null;
        }

        FinishAction(timeBetweenActions);
    }

    private IEnumerator LeapRoutine()
    {
        if (leapSlam == null)
        {
            FinishAction(0.1f);
            yield break;
        }

        leapSlam.SetLandingObjectEnabled(true);

        if (!leapSlam.TryStartLeap())
        {
            leapSlam.SetLandingObjectEnabled(false);
            FinishAction(0.1f);
            yield break;
        }

        while (leapSlam.IsLeaping && IsAlive())
            yield return null;

        leapSlam.SetLandingObjectEnabled(false);
        FinishAction(timeBetweenActions);
    }

    private IEnumerator ThrowGrenadesRoutine()
    {
        if (!CanThrowGrenade())
        {
            FinishAction(0.1f);
            yield break;
        }

        PauseMovement();
        yield return new WaitForSeconds(grenadeWindup);

        if (!IsAlive() || brain.Target == null)
        {
            ResumeMovement();
            FinishAction(0f);
            yield break;
        }

        FaceTarget();

        int grenadeCount = IsPhaseTwo ? 2 : 1;

        for (int i = 0; i < grenadeCount; i++)
        {
            float angle = grenadeCount == 1
                ? 0f
                : (i == 0 ? -doubleGrenadeSpread : doubleGrenadeSpread);

            ThrowGrenade(angle);
        }

        nextGrenadeTime = Time.time + grenadeCooldown;
        yield return new WaitForSeconds(grenadeRecovery);

        ResumeMovement();
        FinishAction(timeBetweenActions);
    }

    private void ThrowGrenade(float horizontalAngle)
    {
        Vector3 spawnPosition = grenadeThrowPoint != null
            ? grenadeThrowPoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 direction = brain.Target.position - spawnPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        direction = Quaternion.AngleAxis(horizontalAngle, Vector3.up) * direction.normalized;
        Vector3 velocity = direction * grenadeForwardSpeed + Vector3.up * grenadeUpwardSpeed;

        BossBulletGrenade grenade = Instantiate(
            bulletGrenadePrefab,
            spawnPosition,
            Quaternion.identity
        );

        grenade.Initialize(gameObject, velocity);
    }

    private IEnumerator SummonRoutine()
    {
        PauseMovement();
        yield return new WaitForSeconds(summonWindup);

        if (IsAlive())
            SpawnTwoEnemies();

        yield return new WaitForSeconds(summonRecovery);
        ResumeMovement();
        FinishAction(timeBetweenActions);
    }

    private void SpawnTwoEnemies()
    {
        if (summonedEnemyPrefabs == null || summonedEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("BossController: не назначены префабы призываемых врагов.", this);
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject prefab = summonedEnemyPrefabs[i % summonedEnemyPrefabs.Length];
            Vector3 position;
            Quaternion rotation;

            if (summonPoints != null && i < summonPoints.Length && summonPoints[i] != null)
            {
                position = summonPoints[i].position;
                rotation = summonPoints[i].rotation;
            }
            else
            {
                float side = i == 0 ? -1f : 1f;
                position = transform.position + transform.right * side * 2.5f;
                rotation = transform.rotation;
            }

            Instantiate(prefab, position, rotation);
        }
    }

    private bool CanThrowGrenade()
    {
        return bulletGrenadePrefab != null && Time.time >= nextGrenadeTime;
    }

    private bool IsAlive()
    {
        return health == null || !health.IsDead;
    }

    private float GetDistanceToTarget()
    {
        Vector3 difference = brain.Target.position - transform.position;
        difference.y = 0f;
        return difference.magnitude;
    }

    private void FaceTarget()
    {
        Vector3 direction = brain.Target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void PauseMovement()
    {
        if (movement == null || movementPaused)
            return;

        movementWasEnabled = movement.enabled;
        movement.Stop();
        movement.enabled = false;
        movementPaused = true;
    }

    private void ResumeMovement()
    {
        if (movement == null || !movementPaused)
            return;

        if (IsAlive())
        {
            movement.enabled = movementWasEnabled;
            movement.Stop();
        }

        movementPaused = false;
    }

    private void FinishAction(float delay)
    {
        nextActionTime = Time.time + delay;
        isPerformingAction = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        leapSlam?.SetLandingObjectEnabled(false);
        ResumeMovement();
        isPerformingAction = false;
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyRetreatDash : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float triggerDistance = 4f;
    [SerializeField] private float windupTime = 0.15f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float cooldown = 2f;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyRangedCombat rangedCombat;
    [Header("Optional Drop")]
[SerializeField] private GameObject dropOnDashPrefab;
[SerializeField] private float dropHeightOffset = 0.05f;

    private CharacterController controller;

    private bool isDashing;
    private bool movementWasEnabled;
    private bool combatWasEnabled;
    private float nextDashTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();

        if (rangedCombat == null)
            rangedCombat = GetComponent<EnemyRangedCombat>();
    }

    private void Update()
    {
        if (isDashing || Time.time < nextDashTime)
            return;

        if (enemyHealth != null && enemyHealth.IsDead)
            return;

        if (brain == null || brain.Target == null)
            return;

        if (brain.CurrentState != EnemyBrain.EnemyState.Attack)
            return;

        if (GetDistanceToTarget() <= triggerDistance)
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        DropDashObject();

        movementWasEnabled =
            movement != null && movement.enabled;

        combatWasEnabled =
            rangedCombat != null && rangedCombat.enabled;

        if (movement != null)
        {
            movement.Stop();
            movement.enabled = false;
        }

        if (rangedCombat != null)
            rangedCombat.enabled = false;

        yield return new WaitForSeconds(windupTime);

        if (brain.Target == null ||
            (enemyHealth != null && enemyHealth.IsDead))
        {
            FinishDash();
            yield break;
        }

        Vector3 awayDirection =
            transform.position - brain.Target.position;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.001f)
            awayDirection = -transform.forward;

        awayDirection.Normalize();

        // Продолжает смотреть на игрока во время отхода.
        transform.rotation = Quaternion.LookRotation(
            -awayDirection
        );

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            controller.Move(
                awayDirection *
                dashSpeed *
                Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishDash();
    }
    private void DropDashObject()
{
    if (dropOnDashPrefab == null)
        return;

    Vector3 spawnPosition =
        transform.position +
        Vector3.up * dropHeightOffset;

    Instantiate(
        dropOnDashPrefab,
        spawnPosition,
        Quaternion.identity
    );
}

    private float GetDistanceToTarget()
    {
        Vector3 difference =
            brain.Target.position - transform.position;

        difference.y = 0f;

        return difference.magnitude;
    }

    private void FinishDash()
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

            if (rangedCombat != null)
                rangedCombat.enabled = combatWasEnabled;
        }

        nextDashTime = Time.time + cooldown;
        isDashing = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (isDashing && gameObject.activeInHierarchy)
            FinishDash();
    }
}
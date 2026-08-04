using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyApproachDash : MonoBehaviour
{
    [Header("Control")]
    [SerializeField] private bool automaticControl = true;

    [Header("Distance")]
    [SerializeField] private float minimumDistance = 4f;
    [SerializeField] private float maximumDistance = 10f;
    [SerializeField] private float stoppingDistance = 1.8f;

    [Header("Dash")]
    [SerializeField] private float windupTime = 0.25f;
    [SerializeField] private float dashSpeed = 24f;
    [SerializeField] private float maximumDuration = 0.3f;
    [SerializeField] private float cooldown = 2f;

    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Renderer enemyRenderer;

    [Header("Visual")]
    [SerializeField] private Color dashWarningColor =
        Color.yellow;

    private CharacterController controller;
    private Material enemyMaterial;
    private Color normalColor;

    private bool isDashing;
    private bool movementWasEnabled;
    private float nextDashTime;

    public bool IsDashing => isDashing;
    public bool IsReady => !isDashing && Time.time >= nextDashTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (enemyRenderer != null)
        {
            enemyMaterial = enemyRenderer.material;
            normalColor = enemyMaterial.color;
        }
    }

    private void Update()
    {
        if (!automaticControl)
            return;

        if (isDashing || Time.time < nextDashTime)
            return;

        if (enemyHealth != null && enemyHealth.IsDead)
            return;

        if (brain == null ||
            brain.Target == null ||
            brain.CurrentState != EnemyBrain.EnemyState.Chase)
        {
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance >= minimumDistance &&
            distance <= maximumDistance)
        {
            TryStartDash();
        }
    }

    public void SetAutomaticControl(bool value)
    {
        automaticControl = value;
    }

    public bool TryStartDash()
    {
        if (!IsReady || brain == null || brain.Target == null)
            return false;

        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        StartCoroutine(DashRoutine());
        return true;
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        movementWasEnabled =
            movement != null && movement.enabled;

        if (movement != null)
        {
            movement.Stop();
            movement.enabled = false;
        }

        SetWarning(true);

        yield return new WaitForSeconds(windupTime);

        SetWarning(false);

        if (brain.Target == null ||
            (enemyHealth != null && enemyHealth.IsDead))
        {
            FinishDash();
            yield break;
        }

        Vector3 direction =
            brain.Target.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            FinishDash();
            yield break;
        }

        direction.Normalize();
        transform.rotation = Quaternion.LookRotation(direction);

        float dashDistance =
            distance - stoppingDistance;

        float duration = Mathf.Min(
            dashDistance / dashSpeed,
            maximumDuration
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            controller.Move(
                direction * dashSpeed * Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishDash();
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
        SetWarning(false);

        if (movement != null &&
            (enemyHealth == null || !enemyHealth.IsDead))
        {
            movement.enabled = movementWasEnabled;
            movement.Stop();
        }

        nextDashTime = Time.time + cooldown;
        isDashing = false;
    }

    private void SetWarning(bool active)
    {
        if (enemyMaterial != null)
        {
            enemyMaterial.color = active
                ? dashWarningColor
                : normalColor;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (isDashing)
            FinishDash();
        else
            SetWarning(false);
    }
}
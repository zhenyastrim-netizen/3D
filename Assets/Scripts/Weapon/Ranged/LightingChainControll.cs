using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class LightningChainController : MonoBehaviour
{
    [Header("Chain")]
    [SerializeField, Min(0)]
    private int maximumJumps = 3;

    [SerializeField, Min(0.1f)]
    private float searchRadius = 8f;

    [SerializeField, Range(0f, 1f)]
    private float damageMultiplierPerJump = 0.75f;

    [SerializeField]
    private LayerMask enemyMask;
    [Header("Visual")]
[SerializeField]
private LightningArc lightningArcPrefab;

[SerializeField]
private float arcHeight = 1f;

    public void TriggerChain(
        DamagePart initialPart,
        DamageInfo originalInfo)
    {
        if (initialPart.damage <= 0f)
            return;

        HashSet<EnemyHealth> hitEnemies =
            new HashSet<EnemyHealth>();

        EnemyHealth currentEnemy =
            GetComponent<EnemyHealth>();

        hitEnemies.Add(currentEnemy);

        float currentDamage = initialPart.damage;

        for (int jump = 0;
             jump < maximumJumps;
             jump++)
        {
            EnemyHealth nextEnemy = FindNearestEnemy(
                currentEnemy.transform.position,
                hitEnemies
            );

            if (nextEnemy == null)
                break;

            currentDamage *= damageMultiplierPerJump;

            DamagePart[] parts =
            {
                new DamagePart(
                    DamageType.Lightning,
                    currentDamage
                )
            };

            DamageInfo chainDamage = new DamageInfo(
                parts,
                originalInfo.AttackType,
                originalInfo.IsCritical,
                originalInfo.Source,
                true
            );
CreateArc(
    currentEnemy.transform.position,
    nextEnemy.transform.position
);
            nextEnemy.TakeDamage(chainDamage);

            Debug.Log(
                $"Молния перескочила на " +
                $"{nextEnemy.gameObject.name}: " +
                $"{currentDamage:F1} урона"
            );

            hitEnemies.Add(nextEnemy);
            currentEnemy = nextEnemy;
        }
    }
    private void CreateArc(
    Vector3 start,
    Vector3 end)
{
    if (lightningArcPrefab == null)
        return;

    start += Vector3.up * arcHeight;
    end += Vector3.up * arcHeight;

    LightningArc arc = Instantiate(
        lightningArcPrefab,
        Vector3.zero,
        Quaternion.identity
    );

    arc.Show(start, end);
}

    private EnemyHealth FindNearestEnemy(
        Vector3 center,
        HashSet<EnemyHealth> ignoredEnemies)
    {
        Collider[] colliders = Physics.OverlapSphere(
            center,
            searchRadius,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        EnemyHealth nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in colliders)
        {
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.IsDead ||
                ignoredEnemies.Contains(enemy))
            {
                continue;
            }

            float distance =
                (enemy.transform.position - center)
                .sqrMagnitude;

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestEnemy = enemy;
        }

        return nearestEnemy;
    }
}
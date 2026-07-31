using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private DamageNumberUI prefab;
    [SerializeField] private Transform spawnPoint;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDamageReceived += Spawn;
    }

    private void OnDisable()
    {
        enemyHealth.OnDamageReceived -= Spawn;
    }

    private void Spawn(
        float damage,
        DamageType type,
        bool critical,
        bool secondary)
    {
        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Vector3.up * 2f;

        DamageNumberUI number = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        number.Initialize(
            damage,
            type,
            critical,
            secondary
        );
    }
}
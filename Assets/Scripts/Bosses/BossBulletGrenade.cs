using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BossBulletGrenade : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float fuseTime = 1.1f;
    [SerializeField, Min(1)] private int waveCount = 3;
    [SerializeField, Min(0.05f)] private float timeBetweenWaves = 0.45f;

    [Header("Bullets")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField, Min(1)] private int bulletsPerWave = 12;
    [SerializeField, Min(0.1f)] private float bulletSpeed = 13f;
    [SerializeField, Min(0.1f)] private float bulletLifetime = 5f;
    [SerializeField] private float spawnHeight = 0.35f;
    [SerializeField] private DamagePart[] damageParts =
    {
        new DamagePart(DamageType.Kinetic, 8f)
    };

    [Header("Effects")]
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private ParticleSystem waveEffect;

    private Rigidbody body;
    private Collider grenadeCollider;
    private GameObject source;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grenadeCollider = GetComponent<Collider>();
    }

    public void Initialize(GameObject grenadeSource, Vector3 initialVelocity)
    {
        source = grenadeSource;
        IgnoreSourceCollisions();
        body.linearVelocity = initialVelocity;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(fuseTime);

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
    grenadeCollider.isTrigger = true;
        activationEffect?.Play();
if (activationEffect != null)
    activationEffect.Play();
        for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
{
    FireWave(waveIndex);

    if (waveEffect != null)
        waveEffect.Play();

    if (waveIndex < waveCount - 1)
        yield return new WaitForSeconds(timeBetweenWaves);
}
    }

    private void FireWave(int waveIndex)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("BossBulletGrenade: не назначен Projectile Prefab.", this);
            return;
        }

        float angleStep = 360f / bulletsPerWave;
        float waveOffset = waveIndex % 2 == 0 ? 0f : angleStep * 0.5f;
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = (waveOffset + angleStep * i) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(
    Mathf.Cos(angle),
    0f,
    Mathf.Sin(angle)
);
            Vector3 bulletSpawnPosition =
    spawnPosition + direction * 1.2f + Vector3.up * 0.3f;

EnemyProjectile projectile = Instantiate(
    projectilePrefab,
    bulletSpawnPosition,
    Quaternion.LookRotation(direction)
);

projectile.Initialize(
    direction,
    bulletSpeed,
    bulletLifetime,
    CopyDamageParts(),
    source
);Destroy(gameObject, 0.25f);
        }
    }

    private DamagePart[] CopyDamageParts()
    {
        if (damageParts == null)
            return new DamagePart[0];

        DamagePart[] copy = new DamagePart[damageParts.Length];

        for (int i = 0; i < damageParts.Length; i++)
            copy[i] = damageParts[i];

        return copy;
    }

    private void IgnoreSourceCollisions()
    {
        if (source == null || grenadeCollider == null)
            return;

        Collider[] sourceColliders = source.GetComponentsInChildren<Collider>();

        foreach (Collider sourceCollider in sourceColliders)
            Physics.IgnoreCollision(grenadeCollider, sourceCollider);
    }
}
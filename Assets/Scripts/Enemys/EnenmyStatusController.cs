using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyStatusController : MonoBehaviour
{
    [Header("Fire buildup")]
    [SerializeField, Min(1f)]
    private float fireThreshold = 100f;

    [SerializeField, Min(0f)]
    private float fireDecayPerSecond = 8f;

    [Header("Burning")]
    [SerializeField, Min(0f)]
    private float burnDamagePerTick = 5f;

    [SerializeField, Min(0.1f)]
    private float burnDuration = 5f;

    [SerializeField, Min(0.1f)]
    private float burnTickInterval = 1f;

    private EnemyHealth enemyHealth;
    private float fireBuildup;
    private Coroutine burnRoutine;
    [Header("Visual")]
[SerializeField] private ParticleSystem burningEffect;

    public float FireBuildup => fireBuildup;
    public bool IsBurning => burnRoutine != null;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (fireBuildup <= 0f)
            return;

        fireBuildup = Mathf.Max(
            0f,
            fireBuildup -
            fireDecayPerSecond * Time.deltaTime
        );
    }

    public void ApplyBuildup(
        DamagePart damagePart,
        GameObject source)
    {
        if (damagePart.buildup <= 0f)
            return;

        switch (damagePart.damageType)
        {
            case DamageType.Fire:
                AddFireBuildup(
                    damagePart.buildup,
                    source
                );
                break;
        }
    }

    private void AddFireBuildup(
        float amount,
        GameObject source)
    {
        fireBuildup += amount;

        Debug.Log(
            $"Огонь: {fireBuildup:F1}/{fireThreshold:F1}",
            this
        );

        if (fireBuildup < fireThreshold)
            return;

        fireBuildup -= fireThreshold;

        StartBurning(source);
    }

    private void StartBurning(GameObject source)
    {
        if (burningEffect != null)
{
    burningEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );

    burningEffect.Play();
}
if (burningEffect != null)
{
    burningEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmitting
    );
}
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        float spiritMultiplier = 1f;

        if (source != null)
        {
            PlayerStats sourceStats =
                source.GetComponentInParent<PlayerStats>();

            if (sourceStats != null)
            {
                spiritMultiplier = sourceStats.GetValue(
                    StatType.SpiritPower
                );
            }
        }

        float finalBurnDamage =
            burnDamagePerTick * spiritMultiplier;

        burnRoutine = StartCoroutine(
            BurnRoutine(finalBurnDamage, source)
        );
    }

    private IEnumerator BurnRoutine(
        float damagePerTick,
        GameObject source)
    {
        float elapsed = 0f;

        Debug.Log($"{gameObject.name} горит!", this);

        while (elapsed < burnDuration)
        {
            DamagePart[] parts =
            {
                new DamagePart(
                    DamageType.Fire,
                    damagePerTick,
                    0f
                )
            };

            DamageInfo damageInfo = new DamageInfo(
                parts,
                AttackType.Magic,
                false,
                source
            );

            enemyHealth.TakeDamage(damageInfo);

            yield return new WaitForSeconds(
                burnTickInterval
            );

            elapsed += burnTickInterval;
        }

        burnRoutine = null;

        Debug.Log(
            $"{gameObject.name} больше не горит.",
            this
        );
    }

    private void OnDisable()
    {
        if (burningEffect != null)
{
    burningEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );
}
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = null;
    }
}
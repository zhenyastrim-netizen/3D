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
    [Header("Frost buildup")]
[SerializeField, Min(1f)]
private float frostThreshold = 100f;

[SerializeField, Min(0f)]
private float frostDecayPerSecond = 5f;

[SerializeField, Range(0f, 0.9f)]
private float maximumFrostSlow = 0.5f;

[Header("Frost break")]
[SerializeField, Range(0f, 1f)]
private float kineticDefenseReduction = 0.3f;

[SerializeField, Min(0.1f)]
private float frostBreakDuration = 5f;

private float frostBuildup;
private EnemyMovement enemyMovement;
private Coroutine frostBreakRoutine;
    [Header("Visual")]
[SerializeField] private ParticleSystem burningEffect;
[Header("Frost visual")]
[SerializeField]
private ParticleSystem frostBuildupEffect;
[Header("Decay visual")]
[SerializeField]
private ParticleSystem decayEffect;

[SerializeField]
private ParticleSystem frostBreakEffect;
[Header("Decay buildup")]
[SerializeField, Min(1f)]
private float decayThreshold = 100f;

[SerializeField, Min(0f)]
private float decayBuildupDecayPerSecond = 4f;

[Header("Decay status")]
[SerializeField, Min(0f)]
private float decayDamagePerTick = 4f;

[SerializeField, Min(0.1f)]
private float decayDuration = 6f;

[SerializeField, Min(0.1f)]
private float decayTickInterval = 1f;

[SerializeField, Range(0f, 0.9f)]
private float enemyDamageReduction = 0.3f;

private float decayBuildup;
private EnemyCombat enemyCombat;
private Coroutine decayRoutine;

    public float FireBuildup => fireBuildup;
    public bool IsBurning => burnRoutine != null;

    private void Awake()
    {
        enemyCombat = GetComponent<EnemyCombat>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
{
    UpdateDecayBuildup();
    UpdateFireBuildup();
    UpdateFrostBuildup();
}
private void UpdateDecayBuildup()
{
    if (decayBuildup <= 0f)
        return;

    decayBuildup = Mathf.Max(
        0f,
        decayBuildup -
        decayBuildupDecayPerSecond *
        Time.deltaTime
    );
}
private void UpdateFireBuildup()
{
    if (fireBuildup <= 0f)
        return;

    fireBuildup = Mathf.Max(
        0f,
        fireBuildup -
        fireDecayPerSecond * Time.deltaTime
    );
}

private void UpdateFrostBuildup()
{
    if (frostBuildup > 0f)
    {
        frostBuildup = Mathf.Max(
            0f,
            frostBuildup -
            frostDecayPerSecond * Time.deltaTime
        );
    }

    if (enemyMovement == null)
        return;

    float frostRatio = Mathf.Clamp01(
        frostBuildup / frostThreshold
    );

    float speedMultiplier = Mathf.Lerp(
        1f,
        1f - maximumFrostSlow,
        frostRatio
    );

    enemyMovement.SetStatusSpeedMultiplier(
        speedMultiplier
        
    );
    UpdateFrostVisual(frostRatio);
    
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
                case DamageType.Frost:
    AddFrostBuildup(damagePart.buildup);
    break;
    case DamageType.Decay:
    AddDecayBuildup(
        damagePart.buildup,
        source
    );
    break;
                
        }
        
    }
    private void AddDecayBuildup(
    float amount,
    GameObject source)
{
    decayBuildup += amount;

    Debug.Log(
        $"Гниение: {decayBuildup:F1}/{decayThreshold:F1}",
        this
    );

    if (decayBuildup < decayThreshold)
        return;

    decayBuildup -= decayThreshold;

    StartDecay(source);
}

private void StartDecay(GameObject source)
{
    if (decayRoutine != null)
        StopCoroutine(decayRoutine);

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

    float finalTickDamage =
        decayDamagePerTick * spiritMultiplier;

    decayRoutine = StartCoroutine(
        DecayRoutine(finalTickDamage, source)
    );
}
private IEnumerator DecayRoutine(
    float damagePerTick,
    GameObject source)
{
    float elapsed = 0f;
    if (decayEffect != null)
{
    decayEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );

    decayEffect.Play();
}

    if (enemyCombat != null)
    {
        enemyCombat.SetStatusDamageMultiplier(
            1f - enemyDamageReduction
        );
    }

    Debug.Log($"{gameObject.name} гниёт!", this);

    while (elapsed < decayDuration)
    {
        DamagePart[] parts =
        {
            new DamagePart(
                DamageType.Decay,
                damagePerTick
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
            decayTickInterval
        );

        elapsed += decayTickInterval;
    }

    enemyCombat?.SetStatusDamageMultiplier(1f);
if (decayEffect != null)
{
    decayEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmitting
    );
}
    decayRoutine = null;

    Debug.Log(
        $"{gameObject.name} больше не гниёт.",
        this
    );
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
private void AddFrostBuildup(float amount)
{
    frostBuildup += amount;

    Debug.Log(
        $"Холод: {frostBuildup:F1}/{frostThreshold:F1}",
        this
    );

    if (frostBuildup < frostThreshold)
        return;

    frostBuildup -= frostThreshold;

    if (frostBreakRoutine != null)
        StopCoroutine(frostBreakRoutine);

    frostBreakRoutine =
        StartCoroutine(FrostBreakRoutine());
        
}
private void UpdateFrostVisual(float frostRatio)
{
    if (frostBuildupEffect == null)
        return;

    if (frostRatio > 0.01f)
    {
        if (!frostBuildupEffect.isPlaying)
            frostBuildupEffect.Play();
    }
    else
    {
        frostBuildupEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );
    }
}

private IEnumerator FrostBreakRoutine()
{if (frostBreakEffect != null)
{
    frostBreakEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );

    frostBreakEffect.Play();
}
    float defenseMultiplier =
        1f - kineticDefenseReduction;

    enemyHealth.SetKineticDefenseMultiplier(
        defenseMultiplier
    );

    Debug.Log(
        $"{gameObject.name}: кинетическая защита снижена!",
        this
    );

    yield return new WaitForSeconds(
        frostBreakDuration
    );

    enemyHealth.SetKineticDefenseMultiplier(1f);
    if (frostBreakEffect != null)
{
    frostBreakEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmitting
    );
}
    frostBreakRoutine = null;
}
    private void StartBurning(GameObject source)
{
    if (burnRoutine != null)
    {
        StopCoroutine(burnRoutine);
        burnRoutine = null;
    }

    if (burningEffect != null)
    {
        burningEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        burningEffect.Play();
    }

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
        if (burningEffect != null)
{
    burningEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmitting
    );
}

burnRoutine = null;

Debug.Log(
    $"{gameObject.name} больше не горит.",
    this
);

        burnRoutine = null;

        Debug.Log(
            $"{gameObject.name} больше не горит.",
            this
        );
    }

    private void OnDisable()
    {
        if (decayEffect != null)
{
    decayEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );
}
        if (decayRoutine != null)
    StopCoroutine(decayRoutine);

decayRoutine = null;

enemyCombat?.SetStatusDamageMultiplier(1f);
        if (burningEffect != null)
{
    enemyMovement?.SetStatusSpeedMultiplier(1f);
enemyHealth?.SetKineticDefenseMultiplier(1f);

if (frostBuildupEffect != null)
{
    frostBuildupEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );
}

if (frostBreakEffect != null)
{
    frostBreakEffect.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear
    );
}
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
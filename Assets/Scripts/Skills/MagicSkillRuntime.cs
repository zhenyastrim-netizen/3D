using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStats))]
public class MagicSkillRuntime : MonoBehaviour
{
    private PlayerStats playerStats;
    private ElementalSkillRuntime elementalSkills;

    private int magicRank;
    private float manaCostReductionPerRank;
    private float freeCastChancePerRank;

    private int forbiddenRank;
    private float forbiddenMagicDamageBonus;
    private float forbiddenCastSpeedBonus;
    private float forbiddenManaCostIncrease;
    private float fruitChance;
    private int stacksToSummon = 10;
    private float damageTakenPerStack;

    private GameObject ghostPrefab;
    private float ghostMoveSpeed;
    private float ghostDamage;
    private float ghostAttackInterval;
    private float ghostAttackRange;

    private float goodFromEvilDuration;
    private float goodFromEvilMagicDamageBonus;
    private float goodFromEvilStatusDurationBonus;

    private ForbiddenMagicGhost activeGhost;
    private Coroutine buffRoutine;

    public int ForbiddenFruitStacks { get; private set; }
    public bool CanMagicCrit => forbiddenRank > 0;
    public bool HasGoodFromEvilBuff { get; private set; }

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (magicRank > 0 || forbiddenRank > 0)
            RefreshModifiers();
    }

    public void ConfigureMagic(
        int rank,
        float costReductionPerRank,
        float chancePerRank)
    {
        magicRank = Mathf.Max(1, rank);
        manaCostReductionPerRank = Mathf.Clamp01(costReductionPerRank);
        freeCastChancePerRank = Mathf.Clamp01(chancePerRank);

        RefreshModifiers();
    }

    public void ConfigureForbiddenMagic(
        int rank,
        float magicDamageBonus,
        float castSpeedBonus,
        float manaCostIncrease,
        float newFruitChance,
        int newStacksToSummon,
        float newDamageTakenPerStack,
        GameObject newGhostPrefab,
        float newGhostMoveSpeed,
        float newGhostDamage,
        float newGhostAttackInterval,
        float newGhostAttackRange,
        float newBuffDuration,
        float newBuffMagicDamageBonus,
        float newBuffStatusDurationBonus)
    {
        forbiddenRank = Mathf.Max(1, rank);
        forbiddenMagicDamageBonus = Mathf.Max(0f, magicDamageBonus);
        forbiddenCastSpeedBonus = Mathf.Max(0f, castSpeedBonus);
        forbiddenManaCostIncrease = Mathf.Max(0f, manaCostIncrease);
        fruitChance = Mathf.Clamp01(newFruitChance);
        stacksToSummon = Mathf.Max(1, newStacksToSummon);
        damageTakenPerStack = Mathf.Max(0f, newDamageTakenPerStack);

        ghostPrefab = newGhostPrefab;
        ghostMoveSpeed = Mathf.Max(0f, newGhostMoveSpeed);
        ghostDamage = Mathf.Max(0f, newGhostDamage);
        ghostAttackInterval = Mathf.Max(0.05f, newGhostAttackInterval);
        ghostAttackRange = Mathf.Max(0f, newGhostAttackRange);

        goodFromEvilDuration = Mathf.Max(0.1f, newBuffDuration);
        goodFromEvilMagicDamageBonus =
            Mathf.Max(0f, newBuffMagicDamageBonus);
        goodFromEvilStatusDurationBonus =
            Mathf.Max(0f, newBuffStatusDurationBonus);

        ForbiddenFruitStacks = Mathf.Min(
            ForbiddenFruitStacks,
            stacksToSummon
        );

        RefreshModifiers();
    }

    public bool IsNextCastFree()
    {
        if (HasGoodFromEvilBuff)
            return true;

        float chance = Mathf.Clamp01(
            magicRank * freeCastChancePerRank
        );

        return chance > 0f && Random.value < chance;
    }

    public void NotifySpellCast()
    {
        if (forbiddenRank <= 0 || activeGhost != null)
            return;

        if (Random.value >= fruitChance)
            return;

        ForbiddenFruitStacks = Mathf.Min(
            ForbiddenFruitStacks + 1,
            stacksToSummon
        );

        RefreshModifiers();

        if (ForbiddenFruitStacks >= stacksToSummon)
            SummonGhost();
    }

    public void NotifyGhostKilled(ForbiddenMagicGhost ghost)
    {
        if (ghost == null || ghost != activeGhost)
            return;

        activeGhost = null;
        ForbiddenFruitStacks = 0;
        RefreshModifiers();
        StartGoodFromEvilBuff();
    }

    private void SummonGhost()
    {
        Vector3 spawnPosition =
            transform.position +
            transform.right * 2f +
            Vector3.up;

        GameObject ghostObject;

        if (ghostPrefab != null)
        {
            ghostObject = Instantiate(
                ghostPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
        else
        {
            ghostObject = GameObject.CreatePrimitive(
                PrimitiveType.Capsule
            );
            ghostObject.name = "Forbidden Magic Ghost";
            ghostObject.transform.position = spawnPosition;
        }

        ForbiddenMagicGhost ghost =
            ghostObject.GetComponent<ForbiddenMagicGhost>();

        if (ghost == null)
            ghost = ghostObject.AddComponent<ForbiddenMagicGhost>();

        if (ghostObject.GetComponentInChildren<Collider>() == null)
        {
            CapsuleCollider collider =
                ghostObject.AddComponent<CapsuleCollider>();

            collider.height = 2f;
            collider.radius = 0.5f;
        }

        if (ghostObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody body = ghostObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        activeGhost = ghost;

        ghost.Initialize(
            gameObject,
            this,
            ghostMoveSpeed,
            ghostDamage,
            ghostAttackInterval,
            ghostAttackRange
        );
    }

    private void StartGoodFromEvilBuff()
    {
        if (buffRoutine != null)
            StopCoroutine(buffRoutine);

        buffRoutine = StartCoroutine(GoodFromEvilRoutine());
    }

    private IEnumerator GoodFromEvilRoutine()
    {
        HasGoodFromEvilBuff = true;
        SetTemporaryStatusDurationBonus(
            goodFromEvilStatusDurationBonus
        );
        RefreshModifiers();

        yield return new WaitForSeconds(goodFromEvilDuration);

        HasGoodFromEvilBuff = false;
        SetTemporaryStatusDurationBonus(0f);
        RefreshModifiers();
        buffRoutine = null;
    }

    private void RefreshModifiers()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerStats.RemoveModifiersFromSource(this);

        float manaCostPercent =
            forbiddenRank * forbiddenManaCostIncrease -
            magicRank * manaCostReductionPerRank;

        AddPercentModifier(
            StatType.ManaCostMultiplier,
            manaCostPercent
        );

        AddPercentModifier(
            StatType.MagicDamage,
            forbiddenRank * forbiddenMagicDamageBonus +
            (HasGoodFromEvilBuff
                ? goodFromEvilMagicDamageBonus
                : 0f)
        );

        AddPercentModifier(
            StatType.CastSpeed,
            forbiddenRank * forbiddenCastSpeedBonus
        );

        AddPercentModifier(
            StatType.DamageTakenMultiplier,
            ForbiddenFruitStacks * damageTakenPerStack
        );
    }

    private void AddPercentModifier(StatType statType, float value)
    {
        if (Mathf.Approximately(value, 0f))
            return;

        playerStats.AddModifier(
            new StatModifier(
                statType,
                StatModifierType.Percent,
                value,
                this
            )
        );
    }

    private void SetTemporaryStatusDurationBonus(float bonus)
    {
        if (elementalSkills == null)
            elementalSkills = GetComponent<ElementalSkillRuntime>();

        if (elementalSkills == null && bonus > 0f)
            elementalSkills = gameObject.AddComponent<ElementalSkillRuntime>();

        elementalSkills?.SetTemporaryStatusDurationBonus(bonus);
    }

    private void OnDisable()
    {
        if (buffRoutine != null)
        {
            StopCoroutine(buffRoutine);
            buffRoutine = null;
        }

        if (playerStats != null)
            playerStats.RemoveModifiersFromSource(this);

        SetTemporaryStatusDurationBonus(0f);
        HasGoodFromEvilBuff = false;

        if (activeGhost != null)
            Destroy(activeGhost.gameObject);

        activeGhost = null;
    }
}

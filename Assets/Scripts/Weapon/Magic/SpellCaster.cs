using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    [Header("Spell")]
    [SerializeField] private SpellData spell;
    [SerializeField] private Transform castPoint;

    [Header("Input")]
    [SerializeField] private InputAction castAction;

    private PlayerMana playerMana;
    private PlayerStats playerStats;
    private PlayerDamageCalculator damageCalculator;
    private MagicSkillRuntime magicSkills;
    private Camera playerCamera;

    private bool isCasting;
    private float nextCastTime;

    private void Awake()
    {
        playerMana = GetComponentInParent<PlayerMana>();
        playerStats = GetComponentInParent<PlayerStats>();
        damageCalculator =
            GetComponentInParent<PlayerDamageCalculator>();
        magicSkills = GetComponentInParent<MagicSkillRuntime>();

        playerCamera = Camera.main;

        if (castPoint == null)
            castPoint = transform;
    }

    private void OnEnable()
    {
        castAction.Enable();
    }

    private void OnDisable()
    {
        castAction.Disable();
        StopAllCoroutines();
        isCasting = false;
    }

    private void Update()
    {
        if (castAction.WasPressedThisFrame())
            TryCast();
    }

    private void TryCast()
    {
        if (isCasting || Time.time < nextCastTime)
            return;

        if (spell == null ||
            spell.ProjectilePrefab == null)
        {
            return;
        }

        float manaMultiplier = playerStats.GetValue(
            StatType.ManaCostMultiplier
        );

        float finalManaCost =
            spell.ManaCost * manaMultiplier;

        MagicSkillRuntime runtime = GetMagicSkills();

        if (runtime != null && runtime.IsNextCastFree())
            finalManaCost = 0f;

        if (!playerMana.TrySpendMana(finalManaCost))
        {
            Debug.Log("Недостаточно маны");
            return;
        }

        runtime?.NotifySpellCast();

        StartCoroutine(CastRoutine());
    }

    private IEnumerator CastRoutine()
    {
        isCasting = true;

        float castSpeed = Mathf.Max(
            0.01f,
            playerStats.GetValue(StatType.CastSpeed)
        );

        float finalCastTime =
            spell.CastTime / castSpeed;

        MagicSkillRuntime runtime = GetMagicSkills();

        if (runtime != null && runtime.HasGoodFromEvilBuff)
            finalCastTime = 0f;

        if (finalCastTime > 0f)
            yield return new WaitForSeconds(finalCastTime);

        CastProjectile();

        isCasting = false;
        nextCastTime = Time.time + spell.Cooldown;
    }

    private void CastProjectile()
    {
        Vector3 direction =
            playerCamera.transform.forward;

        GameObject projectileObject = Instantiate(
            spell.ProjectilePrefab,
            castPoint.position,
            Quaternion.LookRotation(direction)
        );

        MagicProjectile projectile =
            projectileObject.GetComponent<MagicProjectile>();

        DamagePart[] baseDamage =
        {
            new DamagePart(
                spell.DamageType,
                spell.BaseDamage
            )
        };

        DamageInfo damageInfo =
            damageCalculator.CreateDamage(
                baseDamage,
                AttackType.Magic,
                playerMana.gameObject
            );

        projectile.Initialize(
            spell,
            direction,
            damageInfo,
            playerMana.gameObject
        );
    }

    private MagicSkillRuntime GetMagicSkills()
    {
        if (magicSkills == null)
            magicSkills = GetComponentInParent<MagicSkillRuntime>();

        return magicSkills;
    }
}

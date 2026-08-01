using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSpell",
    menuName = "Combat/Magic/Spell"
)]
public class SpellData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string spellName;
    [SerializeField] private Sprite icon;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float range = 40f;

    [Header("Combat")]
    [SerializeField] private DamageType damageType =
        DamageType.Fire;

    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float manaCost = 10f;
    [SerializeField] private float castTime = 0.2f;
    [SerializeField] private float cooldown = 0.6f;

    public string SpellName => spellName;
    public Sprite Icon => icon;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float Range => range;

    public DamageType DamageType => damageType;
    public float BaseDamage => baseDamage;
    public float ManaCost => manaCost;
    public float CastTime => castTime;
    public float Cooldown => cooldown;
}
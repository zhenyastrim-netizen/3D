using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Legendary")]
    [SerializeField] private bool isLegendary;
    [SerializeField] private string legendaryPropertyName;

    [TextArea]
    [SerializeField] private string legendaryPropertyDescription;

    [Header("Combat")]
    [Tooltip(
        "Каждый элемент наносит свой урон и накопление. " +
        "Можно добавить несколько типов урона одной атаке."
    )]
    [SerializeField] private DamagePart[] damageParts =
        new DamagePart[0];

    // Старые поля оставлены для уже созданных WeaponData.
    // Они используются только пока список Damage Parts пуст.
    [HideInInspector]
    [SerializeField] private float damage = 10f;

    [HideInInspector]
    [SerializeField] private DamageType damageType = DamageType.Kinetic;

    [SerializeField] private float fireRate = 5f;
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private float reloadTime = 1.5f;
    [Header("Melee weapon")]
[SerializeField, Min(0.1f)]
private float meleeRange = 3f;

[SerializeField, Min(0.05f)]
private float meleeHitRadius = 0.6f;

[SerializeField, Min(0.01f)]
private float meleeAttacksPerSecond = 2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject weaponPrefab;
    [Header("Weapon type")]
[SerializeField] private WeaponType weaponType;

public WeaponType WeaponType => weaponType;
public bool IsLegendary => isLegendary;
public string LegendaryPropertyName => legendaryPropertyName;
public string LegendaryPropertyDescription =>
    legendaryPropertyDescription;
[Header("Ranged weapon")]
[SerializeField] private RangedWeaponType rangedWeaponType;
[SerializeField] private WeaponFireMode fireMode;

[Header("Shot")]
[SerializeField, Min(1)] private int projectilesPerShot = 1;
[SerializeField, Min(0f)] private float spreadAngle;
[SerializeField, Min(1)] private int burstSize = 3;
[SerializeField, Min(0f)] private float burstDelay = 0.08f;
[SerializeField, Min(0)] private int penetrationCount;
[SerializeField, Min(0)] private int ricochetCount;
[Header("Ricochet")]
[SerializeField, Min(0f)]
private float ricochetRange = 10f;

[SerializeField, Range(0f, 1f)]
private float ricochetDamageMultiplier = 0.75f;

public float RicochetRange => ricochetRange;
public float RicochetDamageMultiplier =>
    ricochetDamageMultiplier;

public RangedWeaponType RangedWeaponType => rangedWeaponType;
public WeaponFireMode FireMode => fireMode;
public int ProjectilesPerShot => projectilesPerShot;
public float SpreadAngle => spreadAngle;
public int BurstSize => burstSize;
public float BurstDelay => burstDelay;
public int PenetrationCount => penetrationCount;
public int RicochetCount => ricochetCount;
public float MeleeRange => meleeRange;
public float MeleeHitRadius => meleeHitRadius;
public float MeleeAttacksPerSecond => meleeAttacksPerSecond;

    public float Damage
    {
        get
        {
            if (damageParts == null || damageParts.Length == 0)
                return Mathf.Max(0f, damage);

            float totalDamage = 0f;

            foreach (DamagePart part in damageParts)
                totalDamage += Mathf.Max(0f, part.damage);

            return totalDamage;
        }
    }

    public DamageType DamageType =>
        damageParts != null && damageParts.Length > 0
            ? damageParts[0].damageType
            : damageType;

    public DamagePart[] GetDamageParts()
    {
        if (damageParts == null || damageParts.Length == 0)
        {
            return new[]
            {
                new DamagePart(
                    damageType,
                    Mathf.Max(0f, damage)
                )
            };
        }

        DamagePart[] result =
            new DamagePart[damageParts.Length];

        for (int i = 0; i < damageParts.Length; i++)
        {
            DamagePart part = damageParts[i];
            part.damage = Mathf.Max(0f, part.damage);
            part.buildup = Mathf.Max(0f, part.buildup);
            result[i] = part;
        }

        return result;
    }

    public float FireRate => fireRate;
    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public GameObject WeaponPrefab => weaponPrefab;
}
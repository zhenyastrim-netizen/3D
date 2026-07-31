using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Prefabs")]
    [SerializeField] private GameObject weaponPrefab;
    [Header("Weapon type")]
[SerializeField] private WeaponType weaponType;

public WeaponType WeaponType => weaponType;
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

    public float Damage => damage;
    public float FireRate => fireRate;
    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public GameObject WeaponPrefab => weaponPrefab;
}
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

    public float Damage => damage;
    public float FireRate => fireRate;
    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public GameObject WeaponPrefab => weaponPrefab;
}
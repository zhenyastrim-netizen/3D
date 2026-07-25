using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon")]

    [SerializeField] private float damage;

    [SerializeField] private float fireRate;

    [SerializeField] private int magazineSize;

    [SerializeField] private float reloadTime;

    [SerializeField] private GameObject weaponPrefab;

    public float Damage => damage;
    public float FireRate => fireRate;
    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public GameObject WeaponPrefab => weaponPrefab;
}
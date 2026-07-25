using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int reserveAmmo = 60;

    private int currentAmmo;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;

    private void Awake()
    {
        currentAmmo = magazineSize;
    }


    public bool CanShoot()
    {
        return currentAmmo > 0;
    }


    public void UseAmmo()
    {
        if (currentAmmo > 0)
            currentAmmo--;
    }


    public void Reload()
    {
        int needed = magazineSize - currentAmmo;

        int amount = Mathf.Min(
            needed,
            reserveAmmo
        );

        currentAmmo += amount;
        reserveAmmo -= amount;
    }


    public bool CanReload()
    {
        return currentAmmo < magazineSize 
            && reserveAmmo > 0;
    }
}
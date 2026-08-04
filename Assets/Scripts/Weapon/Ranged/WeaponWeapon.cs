using UnityEngine;
using System;
public class WeaponAmmo : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int reserveAmmo = 60;
    public event Action<int, int> OnAmmoChanged;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    private int currentAmmo;
    private int currentMagazineSize;
    private WeaponInstance weaponInstance;
    private bool isInitialized;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MagazineSize => currentMagazineSize;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatChanged += HandleStatChanged;
    }

    private void Start()
    {
        if (isInitialized)
            return;

        currentMagazineSize = CalculateMagazineSize();
        currentAmmo = currentMagazineSize;
        isInitialized = true;
        NotifyAmmoChanged();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatChanged -= HandleStatChanged;
    }

    private int CalculateMagazineSize()
    {
        int bonus = playerStats != null
            ? Mathf.RoundToInt(
                playerStats.GetValue(StatType.MagazineSize)
            )
            : 0;

        return Mathf.Max(1, magazineSize + bonus);
    }

    private void HandleStatChanged(
        StatType statType,
        float newValue)
    {
        if (statType != StatType.MagazineSize)
            return;

        int oldSize = currentMagazineSize;
        currentMagazineSize = CalculateMagazineSize();

        int difference = currentMagazineSize - oldSize;

        if (difference > 0)
        {
            currentAmmo += difference;
        }
        else if (currentAmmo > currentMagazineSize)
        {
            int overflow = currentAmmo - currentMagazineSize;

            currentAmmo = currentMagazineSize;
            reserveAmmo += overflow;
        }

        SaveState();
        NotifyAmmoChanged();
    }

    public void Initialize(WeaponInstance instance)
    {
        weaponInstance = instance;
        currentMagazineSize = CalculateMagazineSize();

        if (weaponInstance != null && weaponInstance.HasAmmoState)
        {
            currentAmmo = Mathf.Clamp(
                weaponInstance.CurrentAmmo,
                0,
                currentMagazineSize
            );

            int overflow = Mathf.Max(
                0,
                weaponInstance.CurrentAmmo - currentMagazineSize
            );

            reserveAmmo = Mathf.Max(
                0,
                weaponInstance.ReserveAmmo + overflow
            );
        }
        else
        {
            currentAmmo = currentMagazineSize;
        }

        isInitialized = true;
        SaveState();
        NotifyAmmoChanged();
    }

    public void DetachInstance()
    {
        SaveState();
        weaponInstance = null;
    }

    public bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public void UseAmmo()
    {
        if (currentAmmo > 0)
            currentAmmo--;

        SaveState();
        NotifyAmmoChanged();
    }

    public void Reload()
    {
        int needed = currentMagazineSize - currentAmmo;
        int amount = Mathf.Min(needed, reserveAmmo);

        currentAmmo += amount;
        reserveAmmo -= amount;
        SaveState();
        NotifyAmmoChanged();
    }

    public bool CanReload()
    {
        return currentAmmo < currentMagazineSize &&
               reserveAmmo > 0;
    }

    private void SaveState()
    {
        weaponInstance?.SaveAmmoState(
            currentAmmo,
            reserveAmmo,
            currentMagazineSize
        );
    }

    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
}
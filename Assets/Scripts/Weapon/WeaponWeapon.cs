using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int reserveAmmo = 60;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    private int currentAmmo;
    private int currentMagazineSize;

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
        currentMagazineSize = CalculateMagazineSize();
        currentAmmo = currentMagazineSize;
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
        int needed = currentMagazineSize - currentAmmo;
        int amount = Mathf.Min(needed, reserveAmmo);

        currentAmmo += amount;
        reserveAmmo -= amount;
    }

    public bool CanReload()
    {
        return currentAmmo < currentMagazineSize &&
               reserveAmmo > 0;
    }
}
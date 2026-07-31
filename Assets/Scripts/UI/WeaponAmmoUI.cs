using TMPro;
using UnityEngine;

public class WeaponAmmoUI : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private TMP_Text ammoText;

    private WeaponAmmo currentAmmo;

    private void Start()
    {
        if (weaponManager == null)
            weaponManager = FindFirstObjectByType<WeaponManager>();

        weaponManager.OnWeaponChanged += HandleWeaponChanged;

        HandleWeaponChanged(
            weaponManager.CurrentWeaponObject
        );
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponChanged -= HandleWeaponChanged;

        UnsubscribeFromAmmo();
    }

    private void HandleWeaponChanged(GameObject weaponObject)
    {
        UnsubscribeFromAmmo();

        if (weaponObject == null)
        {
            ammoText.text = "";
            return;
        }

        currentAmmo =
            weaponObject.GetComponentInChildren<WeaponAmmo>();

        if (currentAmmo == null)
        {
            ammoText.text = "";
            return;
        }

        currentAmmo.OnAmmoChanged += UpdateAmmo;

        UpdateAmmo(
            currentAmmo.CurrentAmmo,
            currentAmmo.ReserveAmmo
        );
    }

    private void UnsubscribeFromAmmo()
    {
        if (currentAmmo != null)
            currentAmmo.OnAmmoChanged -= UpdateAmmo;

        currentAmmo = null;
    }

    private void UpdateAmmo(int magazine, int reserve)
    {
        ammoText.text = $"{magazine} / {reserve}";
    }
}
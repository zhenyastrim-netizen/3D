using UnityEngine;
using UnityEngine.UI;

public class CrosshairReloadUI : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Image reloadCircle;

    private WeaponReload currentReload;

    private void OnEnable()
    {
        weaponManager.OnWeaponChanged += HandleWeaponChanged;

        HandleWeaponChanged(
            weaponManager.CurrentWeaponObject
        );
    }

    private void OnDisable()
    {
        weaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void Update()
    {
        bool isReloading =
            currentReload != null &&
            currentReload.IsReloading;

        reloadCircle.gameObject.SetActive(isReloading);

        if (isReloading)
        {
            reloadCircle.fillAmount =
                currentReload.ReloadProgress;
        }
    }

    private void HandleWeaponChanged(
        GameObject weaponObject)
    {
        currentReload = weaponObject != null
            ? weaponObject.GetComponentInChildren<WeaponReload>(true)
            : null;

        reloadCircle.fillAmount = 0f;
        reloadCircle.gameObject.SetActive(false);
    }
}

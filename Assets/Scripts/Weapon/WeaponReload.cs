using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponReload : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction reloadAction;

    [Header("Settings")]
    [SerializeField] private float reloadTime = 1.5f;

    [Header("References")]
    [SerializeField] private WeaponAmmo ammo;


    public bool IsReloading { get; private set; }


    private void OnEnable()
    {
        reloadAction.Enable();
    }


    private void OnDisable()
    {
        reloadAction.Disable();
    }


    private void Update()
    {
        if (reloadAction.WasPressedThisFrame())
        {
            TryReload();
        }
    }


    public void TryReload()
    {
        if (!IsReloading && ammo.CanReload())
        {
            StartCoroutine(ReloadRoutine());
        }
    }


    private IEnumerator ReloadRoutine()
    {
        IsReloading = true;

        Debug.Log("Перезарядка...");

        yield return new WaitForSeconds(reloadTime);

        ammo.Reload();

        Debug.Log("Перезаряжено");

        IsReloading = false;
    }
}
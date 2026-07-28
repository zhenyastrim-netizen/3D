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
    [SerializeField] private PlayerStats playerStats;

    public bool IsReloading { get; private set; }

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    private void OnEnable()
    {
        reloadAction.Enable();
    }

    private void OnDisable()
    {
        reloadAction.Disable();

        StopAllCoroutines();
        IsReloading = false;
    }

    private void Update()
    {
        if (reloadAction.WasPressedThisFrame())
            TryReload();
    }

    public void TryReload()
    {
        if (IsReloading || !ammo.CanReload())
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        IsReloading = true;

        float reloadSpeed = playerStats != null
            ? playerStats.GetValue(StatType.ReloadSpeed)
            : 1f;

        reloadSpeed = Mathf.Max(0.01f, reloadSpeed);

        float finalReloadTime = reloadTime / reloadSpeed;

        Debug.Log(
            $"Перезарядка: {finalReloadTime:F2} сек."
        );

        yield return new WaitForSeconds(finalReloadTime);

        ammo.Reload();
        IsReloading = false;

        Debug.Log("Перезаряжено");
    }
}
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponReload : MonoBehaviour
{
    public event Action OnReloadStarted;
    public event Action OnReloadCompleted;

    [Header("Input")]
    [SerializeField] private InputAction reloadAction;

    [Header("Settings")]
    [SerializeField] private float reloadTime = 1.5f;

    [Header("References")]
    [SerializeField] private WeaponAmmo ammo;
    [SerializeField] private PlayerStats playerStats;
    public float ReloadProgress { get; private set; }

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
        ReloadProgress = 0f;
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
    ReloadProgress = 0f;
    OnReloadStarted?.Invoke();

    float reloadSpeed = playerStats != null
        ? playerStats.GetValue(StatType.ReloadSpeed)
        : 1f;

    reloadSpeed = Mathf.Max(0.01f, reloadSpeed);

    float finalReloadTime =
        reloadTime / reloadSpeed;

    float elapsedTime = 0f;

    while (elapsedTime < finalReloadTime)
    {
        elapsedTime += Time.deltaTime;

        ReloadProgress = Mathf.Clamp01(
            elapsedTime / finalReloadTime
        );

        yield return null;
    }

    ReloadProgress = 1f;

    ammo.Reload();
    OnReloadCompleted?.Invoke();

    IsReloading = false;
    ReloadProgress = 0f;
}
}

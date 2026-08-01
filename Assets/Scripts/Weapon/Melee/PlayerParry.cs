using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Input")]
    [SerializeField] private InputAction parryAction;

    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.16f;
    [SerializeField] private float recoveryTime = 0.30f;

    public bool IsParrying { get; private set; }

    private bool canParry = true;

    private void OnEnable()
    {
        parryAction.Enable();
    }

    private void OnDisable()
    {
        parryAction.Disable();

        StopAllCoroutines();

        IsParrying = false;
        canParry = true;
    }

    private void Update()
    {
        if (!parryAction.WasPressedThisFrame())
            return;

        if (!canParry)
            return;

        if (!HasMeleeWeapon())
            return;

        StartCoroutine(ParryRoutine());
    }

    private bool HasMeleeWeapon()
    {
        if (weaponManager == null)
            return false;

        WeaponData weaponData = weaponManager.CurrentWeaponData;

        return weaponData != null &&
               weaponData.WeaponType == WeaponType.Melee;
    }

    private IEnumerator ParryRoutine()
    {
        canParry = false;
        IsParrying = true;

        Debug.Log("Окно парирования открыто");

        yield return new WaitForSeconds(parryWindow);

        IsParrying = false;

        yield return new WaitForSeconds(recoveryTime);

        canParry = true;
    }

    public bool TryParry(GameObject attacker)
    {
        if (!IsParrying)
            return false;

        IsParrying = false;

        Debug.Log($"Атака парирована: {attacker.name}");

        return true;
    }
}
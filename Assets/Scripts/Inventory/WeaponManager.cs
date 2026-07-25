using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction slot1Action;
    [SerializeField] private InputAction slot2Action;
    [SerializeField] private InputAction slot3Action;
    [SerializeField] private InputAction slot4Action;

    [Header("Weapons")]
    [SerializeField] private GameObject[] weapons;

    private int currentWeapon = -1;

    private void OnEnable()
    {
        slot1Action.Enable();
        slot2Action.Enable();
        slot3Action.Enable();
        slot4Action.Enable();
    }

    private void OnDisable()
    {
        slot1Action.Disable();
        slot2Action.Disable();
        slot3Action.Disable();
        slot4Action.Disable();
    }

    private void Start()
    {
        EquipWeapon(0);
    }

    private void Update()
    {
        if (slot1Action.WasPressedThisFrame())
            EquipWeapon(0);

        if (slot2Action.WasPressedThisFrame())
            EquipWeapon(1);

        if (slot3Action.WasPressedThisFrame())
            EquipWeapon(2);

        if (slot4Action.WasPressedThisFrame())
            EquipWeapon(3);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
            return;

        if (currentWeapon == index)
            return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        currentWeapon = index;

        Debug.Log("Equipped: " + weapons[index].name);
    }
}
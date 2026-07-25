using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private HotbarController hotbarController;
    [SerializeField] private Transform weaponHolder;

    private GameObject currentWeaponObject;
    private WeaponData currentWeaponData;
    private int equippedSlotIndex = -1;
[SerializeField] private Camera playerCamera;
[SerializeField] private CameraRecoil cameraRecoil;
    public WeaponData CurrentWeaponData => currentWeaponData;
    public GameObject CurrentWeaponObject => currentWeaponObject;

    private void Start()
{
    hotbarController.OnSelectedSlotChanged += HandleSelectedSlotChanged;
    inventory.OnInventoryChanged += HandleInventoryChanged;

    Invoke(nameof(EquipCurrentSlot), 0f);
}

private void EquipCurrentSlot()
{
    EquipFromSlot(hotbarController.SelectedIndex);
}

    private void OnDestroy()
    {
        if (hotbarController != null)
            hotbarController.OnSelectedSlotChanged -= HandleSelectedSlotChanged;

        if (inventory != null)
            inventory.OnInventoryChanged -= HandleInventoryChanged;
    }

    private void HandleSelectedSlotChanged(int slotIndex)
    {
        EquipFromSlot(slotIndex);
    }

    private void HandleInventoryChanged()
    {
        EquipFromSlot(hotbarController.SelectedIndex);
    }

    private void EquipFromSlot(int slotIndex)
    {
        if (inventory == null || inventory.Slots == null)
            return;

        if (slotIndex < 0 || slotIndex >= inventory.Slots.Length)
        {
            UnequipWeapon();
            return;
        }

        InventorySlot slot = inventory.Slots[slotIndex];

        if (slot.IsEmpty)
        {
            UnequipWeapon();
            return;
        }

        WeaponData weaponData = slot.Item.Item as WeaponData;

        if (weaponData == null)
        {
            UnequipWeapon();
            return;
        }

        if (equippedSlotIndex == slotIndex &&
            currentWeaponData == weaponData &&
            currentWeaponObject != null)
        {
            return;
        }

        EquipWeapon(weaponData, slotIndex);
    }

    private void EquipWeapon(WeaponData weaponData, int slotIndex)
{
    UnequipWeapon();

    if (weaponData == null || weaponData.WeaponPrefab == null)
        return;

    currentWeaponObject = Instantiate(
        weaponData.WeaponPrefab,
        weaponHolder
    );
    Debug.Log(
    $"WeaponManager создал {currentWeaponObject.name} " +
    $"в объекте {weaponHolder.name}",
    currentWeaponObject
);

    currentWeaponObject.transform.localPosition = Vector3.zero;
    currentWeaponObject.transform.localRotation = Quaternion.identity;

    currentWeaponData = weaponData;
    equippedSlotIndex = slotIndex;
}


    private void UnequipWeapon()
{
    if (currentWeaponObject != null)
    {
        Destroy(currentWeaponObject);
        currentWeaponObject = null;
    }

    currentWeaponData = null;
    equippedSlotIndex = -1;
}
}
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private HotbarController hotbarController;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private CameraRecoil cameraRecoil;

    private GameObject currentWeaponObject;
    private WeaponData currentWeaponData;
    private WeaponInstance currentWeaponInstance;

    private int equippedSlotIndex = -1;

    public WeaponData CurrentWeaponData =>
        currentWeaponData;

    public WeaponInstance CurrentWeaponInstance =>
        currentWeaponInstance;

    public GameObject CurrentWeaponObject =>
        currentWeaponObject;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats =
                GetComponentInParent<PlayerStats>();
        }

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Start()
    {
        if (hotbarController != null)
        {
            hotbarController.OnSelectedSlotChanged +=
                HandleSelectedSlotChanged;
        }

        if (inventory != null)
        {
            inventory.OnInventoryChanged +=
                HandleInventoryChanged;
        }

        Invoke(nameof(EquipCurrentSlot), 0f);
    }

    private void OnDestroy()
    {
        if (hotbarController != null)
        {
            hotbarController.OnSelectedSlotChanged -=
                HandleSelectedSlotChanged;
        }

        if (inventory != null)
        {
            inventory.OnInventoryChanged -=
                HandleInventoryChanged;
        }

        RemoveWeaponModifiers();
    }

    private void EquipCurrentSlot()
    {
        if (hotbarController == null)
            return;

        EquipFromSlot(
            hotbarController.SelectedIndex
        );
    }

    private void HandleSelectedSlotChanged(
        int slotIndex)
    {
        EquipFromSlot(slotIndex);
    }

    private void HandleInventoryChanged()
    {
        EquipCurrentSlot();
    }

    private void EquipFromSlot(int slotIndex)
    {
        if (inventory == null ||
            inventory.Slots == null)
        {
            return;
        }

        if (slotIndex < 0 ||
            slotIndex >= inventory.Slots.Length)
        {
            UnequipWeapon();
            return;
        }

        InventorySlot slot =
            inventory.Slots[slotIndex];

        if (slot == null || slot.IsEmpty)
        {
            UnequipWeapon();
            return;
        }

        InventoryItem inventoryItem = slot.Item;

        WeaponData weaponData =
            inventoryItem.Item as WeaponData;

        if (weaponData == null)
        {
            UnequipWeapon();
            return;
        }

        WeaponInstance weaponInstance =
            inventoryItem.WeaponInstance;

        if (weaponInstance == null)
        {
            weaponInstance = new WeaponInstance(
                weaponData,
                ItemRarity.Common,
                ItemAlignment.Neutral
            );
        }

        if (equippedSlotIndex == slotIndex &&
            currentWeaponInstance == weaponInstance &&
            currentWeaponObject != null)
        {
            return;
        }

        EquipWeapon(
            weaponInstance,
            slotIndex
        );
    }

    private void EquipWeapon(
        WeaponInstance weapon,
        int slotIndex)
    {
        UnequipWeapon();

        if (weapon == null ||
            weapon.BaseData == null ||
            weapon.BaseData.WeaponPrefab == null)
        {
            return;
        }

        currentWeaponInstance = weapon;
        currentWeaponData = weapon.BaseData;
        equippedSlotIndex = slotIndex;

        currentWeaponObject = Instantiate(
            currentWeaponData.WeaponPrefab,
            weaponHolder
        );

        currentWeaponObject.transform.localPosition =
            Vector3.zero;

        currentWeaponObject.transform.localRotation =
            Quaternion.identity;

        HitscanWeapon hitscan =
            currentWeaponObject
                .GetComponentInChildren<HitscanWeapon>();

        if (hitscan != null)
        {
            hitscan.Initialize(
                playerCamera,
                cameraRecoil
            );
        }

        ApplyWeaponModifiers();

        Debug.Log(
            $"Экипировано: {currentWeaponData.itemName} | " +
            $"{weapon.Rarity} | {weapon.Alignment} | " +
            $"аффиксов: {weapon.Affixes.Count}"
        );
    }

    private void ApplyWeaponModifiers()
    {
        if (playerStats == null ||
            currentWeaponInstance == null)
        {
            return;
        }

        foreach (WeaponAffix affix
                 in currentWeaponInstance.Affixes)
        {
            StatModifier modifier =
                new StatModifier(
                    affix.StatType,
                    affix.ModifierType,
                    affix.Value,
                    currentWeaponInstance
                );

            playerStats.AddModifier(modifier);
        }
    }

    private void RemoveWeaponModifiers()
    {
        if (playerStats == null ||
            currentWeaponInstance == null)
        {
            return;
        }

        playerStats.RemoveModifiersFromSource(
            currentWeaponInstance
        );
    }

    private void UnequipWeapon()
    {
        RemoveWeaponModifiers();

        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
        }

        currentWeaponInstance = null;
        currentWeaponData = null;
        equippedSlotIndex = -1;
    }
}
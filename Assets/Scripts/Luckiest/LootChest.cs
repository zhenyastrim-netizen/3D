using UnityEngine;
using UnityEngine.InputSystem;

public class LootChest : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction interactAction;

    [Header("Loot")]
    [SerializeField] private WeaponLootGenerator generator;
    [SerializeField] private WeaponData[] possibleWeapons;

    [SerializeField, Min(1)]
    private int minimumDrops = 1;

    [SerializeField, Min(1)]
    private int maximumDrops = 1;

    [Header("Visual")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";

    private PlayerInventories playerInventories;
    private bool playerInRange;
    private bool isOpened;

    private void Awake()
    {
        if (generator == null)
        {
            generator =
                FindFirstObjectByType<WeaponLootGenerator>();
        }
    }

    private void OnEnable()
    {
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
    }

    private void Update()
    {
        if (!playerInRange || isOpened)
            return;

        if (interactAction.WasPressedThisFrame())
            OpenChest();
    }

    private void OpenChest()
    {
        if (generator == null ||
            playerInventories == null ||
            possibleWeapons == null ||
            possibleWeapons.Length == 0)
        {
            Debug.LogWarning(
                "Сундук настроен не полностью.",
                this
            );

            return;
        }

        int dropCount = Random.Range(
            minimumDrops,
            maximumDrops + 1
        );

        int addedCount = 0;

        for (int i = 0; i < dropCount; i++)
        {
            WeaponData selectedWeapon =
                possibleWeapons[
                    Random.Range(
                        0,
                        possibleWeapons.Length
                    )
                ];

            if (selectedWeapon == null)
                continue;

            WeaponInstance generatedWeapon =
                generator.GenerateRandom(
                    selectedWeapon
                );

            bool added =
                playerInventories.Main.AddWeapon(
                    generatedWeapon
                );

            if (!added)
                break;

            addedCount++;

            Debug.Log(
                $"Получено оружие: " +
                $"{selectedWeapon.itemName} | " +
                $"{generatedWeapon.Rarity} | " +
                $"{generatedWeapon.Alignment} | " +
                $"бонусов: {generatedWeapon.Affixes.Count}"
            );
        }

        if (addedCount <= 0)
            return;

        isOpened = true;

        if (animator != null)
            animator.SetTrigger(openTrigger);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventories inventories =
            other.GetComponentInParent<PlayerInventories>();

        if (inventories == null)
            return;

        playerInventories = inventories;
        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInventories inventories =
            other.GetComponentInParent<PlayerInventories>();

        if (inventories == null ||
            inventories != playerInventories)
        {
            return;
        }

        playerInRange = false;
        playerInventories = null;
    }
}
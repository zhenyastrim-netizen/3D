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

    [Header("Drop")]
    [SerializeField] private WorldWeaponDrop dropPrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropForce = 4f;

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
            dropPrefab == null ||
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

        int spawnedCount = 0;

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

            if (generatedWeapon == null)
                continue;

            Vector3 spawnPosition =
                dropPoint != null
                    ? dropPoint.position
                    : transform.position +
                      Vector3.up;

            WorldWeaponDrop drop = Instantiate(
                dropPrefab,
                spawnPosition,
                Quaternion.identity
            );

            drop.Initialize(generatedWeapon);

            Rigidbody body =
                drop.GetComponent<Rigidbody>();

            if (body != null)
{
    Vector2 random =
        Random.insideUnitCircle * 0.35f;

    Vector3 direction = new Vector3(
        random.x,
        0.8f,
        random.y
    );

    body.AddForce(
        direction * dropForce,
        ForceMode.Impulse
    );
}

            spawnedCount++;

            Debug.Log(
                $"Выпало оружие: " +
                $"{selectedWeapon.itemName} | " +
                $"{generatedWeapon.Rarity} | " +
                $"{generatedWeapon.Alignment} | " +
                $"аффиксов: " +
                $"{generatedWeapon.Affixes.Count}"
            );
        }

        if (spawnedCount <= 0)
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

    private void OnValidate()
    {
        maximumDrops = Mathf.Max(
            maximumDrops,
            minimumDrops
        );
    }
}
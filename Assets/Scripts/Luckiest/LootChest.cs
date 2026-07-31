using UnityEngine;
using UnityEngine.InputSystem;

public class LootChest : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction interactAction;

    [Header("Loot pool")]
    [SerializeField] private LootEntry[] possibleLoot;

    [SerializeField, Min(1)]
    private int minimumDrops = 1;

    [SerializeField, Min(1)]
    private int maximumDrops = 3;

    [Header("Weapon")]
    [SerializeField] private WeaponLootGenerator weaponGenerator;
    [SerializeField] private WorldWeaponDrop weaponDropPrefab;

    [Header("Other items")]
    [SerializeField] private WorldItemDrop itemDropPrefab;

    [Header("Drop")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropForce = 2f;

    [Header("Visual")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";

    private bool playerInRange;
    private bool isOpened;

    private void Awake()
    {
        if (weaponGenerator == null)
        {
            weaponGenerator =
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
        if (possibleLoot == null ||
            possibleLoot.Length == 0)
        {
            Debug.LogWarning(
                "В сундуке отсутствует пул лута.",
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
            LootEntry entry = RollLoot();

            if (entry == null || entry.item == null)
                continue;

            bool spawned = entry.item is WeaponData weapon
                ? SpawnWeapon(weapon)
                : SpawnItem(entry);

            if (spawned)
                spawnedCount++;
        }

        if (spawnedCount <= 0)
            return;

        isOpened = true;

        if (animator != null)
            animator.SetTrigger(openTrigger);
    }

    private LootEntry RollLoot()
    {
        float totalWeight = 0f;

        foreach (LootEntry entry in possibleLoot)
        {
            if (entry != null && entry.item != null)
                totalWeight += Mathf.Max(0f, entry.weight);
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);

        foreach (LootEntry entry in possibleLoot)
        {
            if (entry == null || entry.item == null)
                continue;

            roll -= Mathf.Max(0f, entry.weight);

            if (roll <= 0f)
                return entry;
        }

        return null;
    }

    private bool SpawnWeapon(WeaponData weapon)
    {
        if (weaponGenerator == null ||
            weaponDropPrefab == null)
        {
            return false;
        }

        WeaponInstance instance =
            weaponGenerator.GenerateRandom(weapon);

        if (instance == null)
            return false;

        WorldWeaponDrop drop = Instantiate(
            weaponDropPrefab,
            GetDropPosition(),
            Quaternion.identity
        );

        drop.Initialize(instance);
        ApplyDropForce(drop.GetComponent<Rigidbody>());

        return true;
    }

    private bool SpawnItem(LootEntry entry)
    {
        if (itemDropPrefab == null)
            return false;

        int maximum = Mathf.Max(
            entry.minimumAmount,
            entry.maximumAmount
        );

        int amount = Random.Range(
            entry.minimumAmount,
            maximum + 1
        );

        WorldItemDrop drop = Instantiate(
            itemDropPrefab,
            GetDropPosition(),
            Quaternion.identity
        );

        drop.Initialize(entry.item, amount);
        ApplyDropForce(drop.GetComponent<Rigidbody>());

        return true;
    }

    private Vector3 GetDropPosition()
    {
        Vector3 basePosition = dropPoint != null
            ? dropPoint.position
            : transform.position + Vector3.up;

        Vector2 offset =
            Random.insideUnitCircle * 0.25f;

        return basePosition +
            new Vector3(offset.x, 0f, offset.y);
    }

    private void ApplyDropForce(Rigidbody body)
    {
        if (body == null)
            return;

        Vector2 random =
            Random.insideUnitCircle * 0.3f;

        Vector3 direction = new Vector3(
            random.x,
            0.65f,
            random.y
        );

        body.AddForce(
            direction.normalized * dropForce,
            ForceMode.Impulse
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerInventories>() != null)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerInventories>() != null)
            playerInRange = false;
    }

    private void OnValidate()
    {
        maximumDrops = Mathf.Max(
            maximumDrops,
            minimumDrops
        );
    }
}
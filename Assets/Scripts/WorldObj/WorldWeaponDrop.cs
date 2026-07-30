using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldWeaponDrop : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction pickupAction;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private ParticleSystem glowEffect;
    [SerializeField] private LineRenderer rarityBeam;
    [SerializeField] private Light rarityLight;
    [SerializeField] private TMP_Text itemNameText;
[SerializeField, Min(0f)]
private float pickupDelay = 0.5f;
[Header("Alignment effects")]
[SerializeField] private ParticleSystem sanctifiedEffect;
[SerializeField] private ParticleSystem cursedEffect;

[Header("Light pulse")]
[SerializeField] private float pulseSpeed = 3f;

private float baseLightIntensity;
private float lightPulseAmount;
private float pickupAvailableTime;
    private WeaponInstance weaponInstance;
    private PlayerInventories playerInventories;
    private GameObject spawnedModel;
    private bool playerInRange;

    public void Initialize(WeaponInstance instance)
    {
        weaponInstance = instance;

        if (instance == null ||
            instance.BaseData == null)
        {
            Destroy(gameObject);
            return;
        }

        SpawnModel();
        UpdateVisuals();
        pickupAvailableTime =
    Time.time + pickupDelay;
    }

    private void OnEnable()
    {
        pickupAction.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
    }

    private void Update()
{
    UpdateLightPulse();
    if (!playerInRange ||
        weaponInstance == null)
    {
        return;
    }

    if (Time.time < pickupAvailableTime)
        return;

    if (pickupAction.WasPressedThisFrame())
        TryPickup();
}

    private void SpawnModel()
    {
        GameObject worldPrefab =
            weaponInstance.BaseData.worldPrefab;

        if (worldPrefab == null ||
            visualRoot == null)
        {
            return;
        }

        spawnedModel = Instantiate(
            worldPrefab,
            visualRoot
        );

        spawnedModel.transform.localPosition =
            Vector3.zero;

        spawnedModel.transform.localRotation =
            Quaternion.identity;
    }

    private void UpdateVisuals()
{
    Color color;
    float beamHeight;
    float beamWidth;
    float particleRate;
    float lightIntensity;
    bool showBeam;

    switch (weaponInstance.Rarity)
    {
        case ItemRarity.Rare:
            color = new Color(0.15f, 0.45f, 1f);
            beamHeight = 2.5f;
            beamWidth = 0.06f;
            particleRate = 10f;
            lightIntensity = 1.5f;
            lightPulseAmount = 0.25f;
            showBeam = true;
            break;

        case ItemRarity.Legendary:
            color = new Color(1f, 0.4f, 0.03f);
            beamHeight = 4f;
            beamWidth = 0.1f;
            particleRate = 25f;
            lightIntensity = 2.5f;
            lightPulseAmount = 0.7f;
            showBeam = true;
            break;

        case ItemRarity.Unique:
            color = new Color(0.7f, 0.1f, 1f);
            beamHeight = 5f;
            beamWidth = 0.14f;
            particleRate = 40f;
            lightIntensity = 3.5f;
            lightPulseAmount = 1f;
            showBeam = true;
            break;

        default:
            color = new Color(0.7f, 0.7f, 0.7f);
            beamHeight = 0f;
            beamWidth = 0f;
            particleRate = 3f;
            lightIntensity = 0.5f;
            lightPulseAmount = 0f;
            showBeam = false;
            break;
    }

    ApplyAlignmentVisual(ref color);
    ApplyGlow(color, particleRate);
    ApplyBeam(color, beamHeight, beamWidth, showBeam);
    ApplyLight(color, lightIntensity);
    UpdateLabel(color);
}

    private void TryPickup()
    {
        if (playerInventories == null)
            return;

        bool added =
            playerInventories.Main.AddWeapon(
                weaponInstance
            );

        if (added)
            Destroy(gameObject);
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare:
                return new Color(0.2f, 0.5f, 1f);

            case ItemRarity.Legendary:
                return new Color(1f, 0.45f, 0.05f);

            case ItemRarity.Unique:
                return new Color(0.75f, 0.2f, 1f);

            default:
                return new Color(0.75f, 0.75f, 0.75f);
        }
    }
    private void ApplyAlignmentVisual(ref Color color)
{
    if (sanctifiedEffect != null)
    {
        sanctifiedEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    if (cursedEffect != null)
    {
        cursedEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    switch (weaponInstance.Alignment)
    {
        case ItemAlignment.Sanctified:
            color = Color.Lerp(
                color,
                new Color(1f, 0.9f, 0.4f),
                0.5f
            );

            sanctifiedEffect?.Play();
            break;

        case ItemAlignment.Cursed:
            color = Color.Lerp(
                color,
                new Color(0.45f, 0.02f, 0.7f),
                0.55f
            );

            cursedEffect?.Play();
            break;
    }
}

private void ApplyGlow(Color color, float rate)
{
    if (glowEffect == null)
        return;

    ParticleSystem.MainModule main =
        glowEffect.main;

    ParticleSystem.EmissionModule emission =
        glowEffect.emission;

    main.startColor = color;
    emission.rateOverTime = rate;

    glowEffect.Play();
}

private void ApplyBeam(
    Color color,
    float height,
    float width,
    bool visible)
{
    if (rarityBeam == null)
        return;

    rarityBeam.enabled = visible;

    if (!visible)
        return;

    rarityBeam.useWorldSpace = false;
    rarityBeam.positionCount = 2;

    rarityBeam.SetPosition(0, Vector3.zero);
    rarityBeam.SetPosition(
        1,
        Vector3.up * height
    );

    rarityBeam.startWidth = width;
    rarityBeam.endWidth = width * 0.25f;

    rarityBeam.startColor = color;
    rarityBeam.endColor = new Color(
        color.r,
        color.g,
        color.b,
        0f
    );
}

private void ApplyLight(
    Color color,
    float intensity)
{
    if (rarityLight == null)
        return;

    rarityLight.color = color;
    baseLightIntensity = intensity;
    rarityLight.intensity = intensity;
}

private void UpdateLightPulse()
{
    if (rarityLight == null ||
        lightPulseAmount <= 0f)
    {
        return;
    }

    float pulse =
        (Mathf.Sin(Time.time * pulseSpeed) + 1f)
        * 0.5f;

    rarityLight.intensity =
        baseLightIntensity +
        pulse * lightPulseAmount;
}

private void UpdateLabel(Color color)
{
    if (itemNameText == null)
        return;

    itemNameText.text =
        $"{weaponInstance.BaseData.itemName}\n" +
        $"{GetRarityName(weaponInstance.Rarity)} " +
        $"{GetAlignmentName(weaponInstance.Alignment)}";

    itemNameText.color = color;
}

    private string GetRarityName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare:
                return "Редкое";

            case ItemRarity.Legendary:
                return "Легендарное";

            case ItemRarity.Unique:
                return "Уникальное";

            default:
                return "Обычное";
        }
    }

    private string GetAlignmentName(
        ItemAlignment alignment)
    {
        switch (alignment)
        {
            case ItemAlignment.Sanctified:
                return "освящённое";

            case ItemAlignment.Cursed:
                return "проклятое";

            default:
                return "";
        }
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

        if (inventories != playerInventories)
            return;

        playerInventories = null;
        playerInRange = false;
    }
}
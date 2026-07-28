using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class PassiveInventoryEffects : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    private Inventory passiveInventory;

    private readonly List<StatEffectInstance> activeEffects =
        new List<StatEffectInstance>();

    private void Awake()
    {
        passiveInventory = GetComponent<Inventory>();

        if (playerStats == null)
        {
            playerStats =
                GetComponentInParent<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        passiveInventory.OnInventoryChanged += RebuildEffects;
    }

    private void Start()
    {
        RebuildEffects();
    }

    private void OnDisable()
    {
        passiveInventory.OnInventoryChanged -= RebuildEffects;
    }

    private void OnDestroy()
    {
        RemoveActiveEffects();
    }

    private void RebuildEffects()
    {
        RemoveActiveEffects();

        foreach (InventorySlot slot
                 in passiveInventory.Slots)
        {
            if (slot == null || slot.IsEmpty)
                continue;

            InventoryItem inventoryItem = slot.Item;
            ItemData item = inventoryItem.Item;

            if (item == null ||
                item.itemType != ItemType.Passive)
            {
                continue;
            }

            int applicationCount =
                item.passiveCanStack
                    ? inventoryItem.Amount
                    : 1;

            for (int i = 0; i < applicationCount; i++)
            {
                ApplyItemEffects(item);
            }
        }
    }

    private void ApplyItemEffects(ItemData item)
    {
        if (item.effects == null)
            return;

        foreach (StatEffect effect in item.effects)
        {
            if (effect == null)
                continue;

            StatEffectInstance instance =
                effect.Apply(playerStats);

            if (instance != null)
                activeEffects.Add(instance);
        }
    }

    private void RemoveActiveEffects()
    {
        foreach (StatEffectInstance effect
                 in activeEffects)
        {
            effect?.Remove();
        }

        activeEffects.Clear();
    }
}
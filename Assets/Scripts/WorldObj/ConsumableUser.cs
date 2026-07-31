using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStats))]
public class ConsumableUser : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerStats playerStats;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerStats = GetComponent<PlayerStats>();
    }

    public bool Use(
        Inventory inventory,
        int slotIndex)
    {
        if (inventory == null)
            return false;

        InventoryItem inventoryItem =
            inventory.GetItem(slotIndex);

        if (inventoryItem == null)
            return false;

        ItemData item = inventoryItem.Item;

        if (item == null ||
            item.itemType != ItemType.Consumable)
        {
            return false;
        }

        bool applied = false;

        if (item is ConsumableData consumable &&
            consumable.HealAmount > 0f)
        {
            playerHealth.Heal(
                consumable.HealAmount
            );

            applied = true;
        }

        if (item.effects != null)
        {
            foreach (StatEffect effect in item.effects)
            {
                if (effect == null)
                    continue;

                StatEffectInstance instance =
                    effect.Apply(playerStats);

                if (instance == null)
                    continue;

                StartCoroutine(
                    RemoveEffectAfterTime(
                        instance,
                        item.effectDuration
                    )
                );

                applied = true;
            }
        }

        if (!applied)
            return false;

        inventory.RemoveItem(slotIndex, 1);
        return true;
    }

    private IEnumerator RemoveEffectAfterTime(
        StatEffectInstance effect,
        float duration)
    {
        yield return new WaitForSeconds(
            Mathf.Max(0.1f, duration)
        );

        effect.Remove();
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("General")]
    public string itemName;

    [TextArea]
    public string description;

    public Sprite icon;

    public ItemType itemType;

    [Header("World")]
    public GameObject worldPrefab;

    [Header("Inventory")]
    public bool stackable;
    public int maxStack = 1;

    

    [Header("Consumable")]
    public float effectDuration = 10f;

    [Header("Healing Flask")]
   
    public int maxCharges = 3;

    [Header("Passive")]
    public bool passiveCanStack = true;

    [Header("Path Effect")]
    public PathEffectType pathEffectType;

    [Header("Quest")]
    public string questItemId;
    [Header("Effects")]
public StatEffect[] effects;
    
}
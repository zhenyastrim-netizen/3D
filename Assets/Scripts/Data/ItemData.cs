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
}
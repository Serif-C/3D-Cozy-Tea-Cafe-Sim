using UnityEngine;


public enum ShopItemType
{
    Unlock,         // Unlocks progression items
    InventoryItem   // Adds a prefab to inventory
}

public abstract class ShopItemDefinition : ScriptableObject
{
    [Header("Shop Info")]
    public string displayName;
    public int cost;
    public Sprite icon;

    [Header("Type")]
    public ShopItemType itemType;

    [Header("Category")]
    public ShopCategory category;

    [Header("Unlock")]
    public UnlockRequirementSO unlockRequirement; // contains unlocksItem

    [Header("Inventory Purchase")]
    public GameObject inventoryPrefab;  // decor, plant, appliance, etc.
}

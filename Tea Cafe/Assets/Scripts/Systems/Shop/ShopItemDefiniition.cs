using UnityEngine;

public abstract class ShopItemDefinition : ScriptableObject
{
    [Header("Shop Info")]
    public string displayName;
    public int cost;
    public Sprite icon;

    [Header("Unlock")]
    public UnlockRequirementSO unlockRequirement; // contains unlocksItem
}

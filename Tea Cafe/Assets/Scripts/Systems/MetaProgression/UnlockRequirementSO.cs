using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Unlock Requirement")]
public class UnlockRequirementSO : ScriptableObject
{
    public UnlockableItemSO unlocksItem;

    public int requiredRank;
    public int requiredReputation;
    public int requiredHappyCustomers;

    public bool unlockViaShopOnly;
}

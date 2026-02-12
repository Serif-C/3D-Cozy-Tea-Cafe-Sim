using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Unlock Requirement")]
public class UnlockRequirementSO : ScriptableObject
{
    public List<UnlockableItemSO> unlocksItems;

    public int requiredRank;
    public int requiredReputation;
    public int requiredHappyCustomers;

    public bool unlockViaShopOnly;
}

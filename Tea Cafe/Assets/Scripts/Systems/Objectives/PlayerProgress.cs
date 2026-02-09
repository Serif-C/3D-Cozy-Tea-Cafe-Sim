using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerProgress",
    menuName = "Progression/Player Progress"
)]
public class PlayerProgress : ScriptableObject
{
    public CafeRank cafeRank = new();
    public CafeReputation cafeReputation = new();
    [SerializeField] private List<DailyGoalDefinitionSO> allDailyGoalDefinitions;

    [Header("Unlocks")]
    [SerializeField] private List<UnlockRequirementSO> allUnlockRequirements;
    private HashSet<string> unlockedItemIDs = new();

    [Header("Meta Progression")]
    public int highestDayReached;

    public void EvaluateUnlocks()
    {
        foreach (var unlock in allUnlockRequirements)
        {
            if (Meets(unlock))
                Unlock(unlock.unlocksItem);
        }

    }

    public bool Meets(UnlockRequirementSO unlock)
    {
        // Should update later to include more conditions not only CafeRank and CafeRep
        // Probably use switch statements for different Meets conditions
        // E.G., Number of Happy Customers Served etc...
        return cafeRank.Level >= unlock.requiredRank
            && cafeReputation.Value >= unlock.requiredReputation;
    }

    public void Unlock(UnlockableItemSO item)
    {
        if (unlockedItemIDs.Add(item.id))
            Debug.Log($"Unlocked: {item.name}");
    }

    public bool IsUnlocked(UnlockableItemSO item)
        => unlockedItemIDs.Contains(item.id);

    public IEnumerable<DailyGoalDefinitionSO> GetUnlockedDailyGoals()
    {
        foreach (var goal in allDailyGoalDefinitions)
        {
            if (goal.unlockRequirement == null || Meets(goal.unlockRequirement))
                yield return goal;
        }
    }
}

using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerProgress",
    menuName = "Progression/Player Progress"
)]
public class PlayerProgress : ScriptableObject
{
    public CafeRank cafeRank = new();
    public CafeReputation cafeReputation = new();

    [Header("Unlocks")]
    public bool hasUnlockedSatisfaction;
    public bool hasUnlockedMeals;
    public bool hasUnlockedParties;

    [Header("Meta Progression")]
    public int highestDayReached;

    public void EvaluateUnlocks()
    {
        if (!hasUnlockedSatisfaction && cafeRank.Level >= 2)
            hasUnlockedSatisfaction = true;
    }
}

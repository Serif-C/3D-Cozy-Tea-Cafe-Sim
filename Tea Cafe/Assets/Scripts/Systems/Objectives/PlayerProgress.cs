using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerProgress",
    menuName = "Progression/Player Progress"
)]
public class PlayerProgress : ScriptableObject
{
    [Header("Unlocks")]
    public bool hasUnlockedSatisfaction;
    public bool hasUnlockedMeals;
    public bool hasUnlockedParties;

    [Header("Meta Progression")]
    public int highestDayReached;
}

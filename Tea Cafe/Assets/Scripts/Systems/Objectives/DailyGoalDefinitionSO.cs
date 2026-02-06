using UnityEngine;

public enum DailyGoalType
{
    ServeCustomers,
    EarnMoney,
    AverageCustomerMood,
    EnragedCustomers
}

[CreateAssetMenu(menuName = "Daily Goals/Daily Goal Definition")]
public class DailyGoalDefinitionSO : ScriptableObject
{
    public string goalID;
    public DailyGoalType goalType;
    public bool alwaysInclude;

    [Header("Base Values")]
    public int baseAmount;

    [Header("Scaling")]
    public AnimationCurve scalingByDay;
    // X = day, Y = multiplier

    [Header("Unlock Requirement")]
    public UnlockRequirementSO unlockRequirement;
}
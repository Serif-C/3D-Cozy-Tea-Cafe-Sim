using UnityEngine;

public enum DailyGoalType
{
    EarnMoney,
    ServeCustomers,
    AverageSatisfaction
}

[CreateAssetMenu(fileName = "DailyGoalDefinition", menuName = "TeaShop/Objectives/Daily Goal Definition", order = 0)]
public class DailyGoalDefinition : ScriptableObject
{
    [Header("Goal Metadata")]
    public DailyGoalType goalType = DailyGoalType.EarnMoney;
    [TextArea(2, 4)]
    public string description = "Earn $100 today";

    [Header("Target + Reward")]
    public int targetValue = 100;
    public int rewardAmount = 25;

    [Header("Unlocking")]
    public int unlockDay = 1;
}

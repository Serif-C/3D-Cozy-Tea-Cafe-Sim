using UnityEngine;

public class EarnMoneyGoal : DailyGoal
{
    IMoneyEarnedSource source;

    public EarnMoneyGoal(IMoneyEarnedSource source, int target)
    {
        this.source = source;
        Target = target;

        Title = "Daily Revenue";
        Description = $"Earn ${Target} today";
    }

    public override void Initialize()
    {
        source.MoneyEarned += OnMoneyEarned;
    }

    public override void CleanUp()
    {
        source.MoneyEarned -= OnMoneyEarned;
    }

    private void OnMoneyEarned(int amount)
    {
        Increment(amount);
        Debug.Log($"[DailyGoal] Money earned +{amount} ({Current}/{Target})");
    }
}

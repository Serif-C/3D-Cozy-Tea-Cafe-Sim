using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DailyGoalFactory
{
    private ICustomerServedSource customerServedSource;
    private IMoneyEarnedSource moneyEarnedSource;
    private PlayerProgress progress;

    [SerializeField] private int baseCustomerGoal = 5;
    [SerializeField] private int baseMoneyGoal = 50;

    public DailyGoalFactory(ICustomerServedSource customerServedSource,
    IMoneyEarnedSource moneyEarnedSource, PlayerProgress progress)
    {
        this.customerServedSource = customerServedSource;
        this.moneyEarnedSource = moneyEarnedSource;
        this.progress = progress;
    }

    public List<DailyGoal> CreateGoals(PlayerProgress progress)
    {
        var goals = new List<DailyGoal>();

        goals.Add(new ServeCustomersGoal(customerServedSource, baseCustomerGoal));
        goals.Add(new EarnMoneyGoal(moneyEarnedSource, baseMoneyGoal));

        if (progress.hasUnlockedSatisfaction)
        {
            // Add satisfaction goal later
        }

        return goals;
    }
}

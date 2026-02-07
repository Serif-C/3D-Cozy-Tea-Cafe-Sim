using System.Collections.Generic;
using UnityEngine;

public class DailyGoalFactory
{
    private readonly ICustomerServedSource customerServedSource;
    private readonly IMoneyEarnedSource moneyEarnedSource;
    private readonly ISatisfactionSource satisfactionSource;
    private readonly PlayerProgress progress;

    public DailyGoalFactory(
        ICustomerServedSource customerServedSource,
        IMoneyEarnedSource moneyEarnedSource,
        ISatisfactionSource satisfactionSource,
        PlayerProgress progress)
    {
        this.customerServedSource = customerServedSource;
        this.moneyEarnedSource = moneyEarnedSource;
        this.satisfactionSource = satisfactionSource;
        this.progress = progress;
    }

    public List<DailyGoal> CreateGoals(int currentDay)
    {
        List<DailyGoal> goals = new();
        var unlocked = new List<DailyGoalDefinitionSO>(
            progress.GetUnlockedDailyGoals()
        );

        // 1. Add mandatory goals
        foreach (var def in unlocked)
        {
            if (def.alwaysInclude)
            {
                goals.Add(CreateGoalFromDefinition(def, currentDay));
            }
        }

        // 2. Add optional/random goals
        int targetCount = DetermineGoalsPerDay(currentDay);
        var optional = unlocked.FindAll(d => !d.alwaysInclude);

        while (goals.Count < targetCount && optional.Count > 0)
        {
            var def = optional[Random.Range(0, optional.Count)];
            optional.Remove(def); // prevent duplicates
            goals.Add(CreateGoalFromDefinition(def, currentDay));
        }

        return goals;
    }

    private DailyGoal CreateGoalFromDefinition(
        DailyGoalDefinitionSO def,
        int day)
    {
        int amount = Mathf.Max(1, Mathf.RoundToInt(
            def.baseAmount *
            def.scalingByDay.Evaluate(day)
        ));

        return def.goalType switch
        {
            DailyGoalType.ServeCustomers =>
                new ServeCustomersGoal(customerServedSource, amount),

            DailyGoalType.EarnMoney =>
                new EarnMoneyGoal(moneyEarnedSource, amount),

            DailyGoalType.AverageCustomerMood =>
                new AverageCustomerSatisfactionGoal(satisfactionSource, amount),

            _ => throw new System.NotImplementedException()
        };
    }

    private int DetermineGoalsPerDay(int day)
    {
        if (day < 5) return 2;
        if (day < 15) return 3;
        return 4;
    }
}

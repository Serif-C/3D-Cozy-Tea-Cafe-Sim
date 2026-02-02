using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DailyGoalManager : MonoBehaviour
{
    public static DailyGoalManager Instance { get; private set; }

    private List<DailyGoal> activeGoals = new();

    private void Awake()
    {
        Instance = this;
    }

    public void StartNewDay(List<DailyGoal> goals)
    {
        CleanUpGoals();

        activeGoals = goals;

        foreach (var goal in activeGoals)
        {
            goal.Initialize();
        }
    }

    public void EndOfDay()
    {
        CleanUpGoals();
    }

    private void CleanUpGoals()
    {
        foreach (var goal in activeGoals)
        {
            goal.CleanUp();
        }

        activeGoals.Clear();
    }

    public IReadOnlyList<DailyGoal> GetGoals() => activeGoals;
}

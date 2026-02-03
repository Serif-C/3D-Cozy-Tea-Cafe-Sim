using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DailyGoalManager : MonoBehaviour
{
    public static DailyGoalManager Instance { get; private set; }

    private List<DailyGoal> activeGoals = new();

    public event Action GoalsUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public float GetCompletionRatio()
    {
        if (activeGoals.Count == 0)
            return 0f;

        float total = 0f;
        foreach (var goal in activeGoals)
            total += goal.Progress01; // 0–1

        return total / activeGoals.Count;
    }


    public void StartNewDay(List<DailyGoal> goals)
    {
        CleanUpGoals();

        activeGoals = goals;

        foreach (var goal in activeGoals)
        {
            goal.Initialize();
        }

        GoalsUpdated?.Invoke();
        Debug.Log($"GoalsUpdated invoked. Goals count: {activeGoals.Count}");
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

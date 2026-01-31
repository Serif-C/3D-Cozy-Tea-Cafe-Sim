using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyGoalsManager : MonoBehaviour
{
    public static DailyGoalsManager Instance { get; private set; }

    [Header("Goal Definitions")]
    [Tooltip("If empty, defaults will be generated at runtime.")]
    [SerializeField] private List<DailyGoalDefinition> goalDefinitions = new();

    [Header("Active Goal Settings")]
    [SerializeField] private int baseActiveGoals = 3;
    [SerializeField] private int daysPerExtraGoal = 3;
    [SerializeField] private int maxActiveGoals = 6;

    [Header("UI")]
    [SerializeField] private DailyGoalsUI goalsUI;

    private readonly List<DailyGoalProgress> activeGoals = new();
    private TimeManager timeManager;
    private int activeDay = 1;
    private bool isFinalizingDay;

    private int moneyEarnedToday;
    private int customersServedToday;
    private float satisfactionTotal;
    private int satisfactionCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (goalsUI == null)
        {
            goalsUI = FindFirstObjectByType<DailyGoalsUI>();
        }

        if (goalsUI == null)
        {
            goalsUI = DailyGoalsUI.CreateDefaultUI();
        }
    }

    private void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        if (timeManager != null)
        {
            activeDay = timeManager.Day;
            timeManager.OnDayChanged += HandleDayChanged;
        }
        else
        {
            Debug.LogWarning("DailyGoalsManager: TimeManager not found, using day 1 defaults.");
            activeDay = 1;
        }

        InitializeDay(activeDay);
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayChanged -= HandleDayChanged;
        }
    }

    private void HandleDayChanged()
    {
        CompleteDay(activeDay);
        activeDay = timeManager != null ? timeManager.Day : activeDay + 1;
        InitializeDay(activeDay);
    }

    private void InitializeDay(int day)
    {
        moneyEarnedToday = 0;
        customersServedToday = 0;
        satisfactionTotal = 0f;
        satisfactionCount = 0;

        activeGoals.Clear();

        List<DailyGoalDefinition> definitions = GetGoalDefinitionsForDay(day);
        int activeGoalCount = Mathf.Min(maxActiveGoals, baseActiveGoals + Mathf.Max(0, (day - 1) / daysPerExtraGoal));

        foreach (DailyGoalDefinition definition in definitions.Take(activeGoalCount))
        {
            activeGoals.Add(new DailyGoalProgress(definition));
        }

        UpdateHud();
    }

    private List<DailyGoalDefinition> GetGoalDefinitionsForDay(int day)
    {
        List<DailyGoalDefinition> definitions = goalDefinitions
            .Where(definition => definition != null && definition.unlockDay <= day)
            .OrderBy(definition => definition.unlockDay)
            .ToList();

        if (definitions.Count == 0)
        {
            definitions = CreateDefaultDefinitions();
        }

        return definitions;
    }

    private List<DailyGoalDefinition> CreateDefaultDefinitions()
    {
        DailyGoalDefinition earnMoney = ScriptableObject.CreateInstance<DailyGoalDefinition>();
        earnMoney.goalType = DailyGoalType.EarnMoney;
        earnMoney.description = "Earn $100 today";
        earnMoney.targetValue = 100;
        earnMoney.rewardAmount = 25;
        earnMoney.unlockDay = 1;

        DailyGoalDefinition serveCustomers = ScriptableObject.CreateInstance<DailyGoalDefinition>();
        serveCustomers.goalType = DailyGoalType.ServeCustomers;
        serveCustomers.description = "Serve 5 customers";
        serveCustomers.targetValue = 5;
        serveCustomers.rewardAmount = 20;
        serveCustomers.unlockDay = 1;

        DailyGoalDefinition satisfaction = ScriptableObject.CreateInstance<DailyGoalDefinition>();
        satisfaction.goalType = DailyGoalType.AverageSatisfaction;
        satisfaction.description = "Maintain average satisfaction above 70%";
        satisfaction.targetValue = 70;
        satisfaction.rewardAmount = 30;
        satisfaction.unlockDay = 1;

        return new List<DailyGoalDefinition> { earnMoney, serveCustomers, satisfaction };
    }

    public void RegisterMoneyEarned(int amount)
    {
        if (amount <= 0 || isFinalizingDay)
        {
            return;
        }

        moneyEarnedToday += amount;
        UpdateHud();
    }

    public void RegisterCustomerServed()
    {
        if (isFinalizingDay)
        {
            return;
        }

        customersServedToday += 1;
        UpdateHud();
    }

    public void RegisterCustomerSatisfaction(float satisfactionValue)
    {
        if (isFinalizingDay)
        {
            return;
        }

        satisfactionTotal += satisfactionValue;
        satisfactionCount += 1;
        UpdateHud();
    }

    private float GetAverageSatisfaction()
    {
        if (satisfactionCount <= 0)
        {
            return 0f;
        }

        return satisfactionTotal / satisfactionCount;
    }

    private void CompleteDay(int dayToSummarize)
    {
        isFinalizingDay = true;

        int totalReward = 0;
        List<string> summaryLines = new()
        {
            $"Day {dayToSummarize} Summary"
        };

        int goalIndex = 1;
        foreach (DailyGoalProgress goal in activeGoals)
        {
            float progressValue = GetProgressValue(goal.Definition.goalType);
            float progressRatio = Mathf.Clamp01(progressValue / Mathf.Max(1f, goal.Definition.targetValue));
            int reward = Mathf.RoundToInt(goal.Definition.rewardAmount * progressRatio);
            totalReward += reward;

            string progressText = FormatGoalProgress(goal.Definition.goalType, progressValue, goal.Definition.targetValue);
            summaryLines.Add($"{goalIndex}. {goal.Definition.description}");
            summaryLines.Add($"   {progressText} | Reward: {reward}");
            goalIndex += 1;
        }

        summaryLines.Add($"Total Reward: {totalReward}");

        if (PlayerManager.Instance != null && totalReward > 0)
        {
            PlayerManager.Instance.SetCountAmount(totalReward);
        }

        if (goalsUI != null)
        {
            goalsUI.ShowSummary(string.Join("\n", summaryLines), HandleSummaryClosed);
        }

        if (timeManager != null)
        {
            timeManager.Pause();
        }

        isFinalizingDay = false;
    }

    private void HandleSummaryClosed()
    {
        if (timeManager != null)
        {
            timeManager.Resume();
        }
    }

    private void UpdateHud()
    {
        if (goalsUI == null)
        {
            return;
        }

        List<string> lines = new()
        {
            $"Daily Goals (Day {activeDay})"
        };

        int goalIndex = 1;
        foreach (DailyGoalProgress goal in activeGoals)
        {
            float progressValue = GetProgressValue(goal.Definition.goalType);
            string progressText = FormatGoalProgress(goal.Definition.goalType, progressValue, goal.Definition.targetValue);
            lines.Add($"{goalIndex}. {goal.Definition.description}");
            lines.Add($"   {progressText}");
            goalIndex += 1;
        }

        goalsUI.SetHudText(string.Join("\n", lines));
    }

    private float GetProgressValue(DailyGoalType goalType)
    {
        return goalType switch
        {
            DailyGoalType.EarnMoney => moneyEarnedToday,
            DailyGoalType.ServeCustomers => customersServedToday,
            DailyGoalType.AverageSatisfaction => GetAverageSatisfaction(),
            _ => 0f
        };
    }

    private string FormatGoalProgress(DailyGoalType goalType, float progressValue, int targetValue)
    {
        return goalType switch
        {
            DailyGoalType.EarnMoney => $"${Mathf.RoundToInt(progressValue)} / ${targetValue}",
            DailyGoalType.ServeCustomers => $"{Mathf.RoundToInt(progressValue)} / {targetValue} served",
            DailyGoalType.AverageSatisfaction => $"{Mathf.RoundToInt(progressValue)}% / {targetValue}%",
            _ => $"{Mathf.RoundToInt(progressValue)} / {targetValue}"
        };
    }

    private class DailyGoalProgress
    {
        public DailyGoalDefinition Definition { get; }

        public DailyGoalProgress(DailyGoalDefinition definition)
        {
            Definition = definition;
        }
    }
}

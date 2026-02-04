using UnityEngine;

public class EndOfDayController : MonoBehaviour
{
    [SerializeField] private PlayerProgress playerProgress;
    public EndOfDayReport LastReport { get; private set; }
    public void OnDayEnded()
    {
        var stats = DailyCafeStats.Instance;
        if (stats == null)
        {
            Debug.LogError("EndOfDayController: DailyCafeStats.Instance is null.");
            return;
        }

        if (playerProgress == null)
        {
            Debug.LogError("EndOfDayController: PlayerProgress reference not set.");
            return;
        }

        // 1) Goal completion ratio (0..1)
        float goalCompletion = 0f;
        if (DailyGoalManager.Instance != null)
            goalCompletion = DailyGoalManager.Instance.GetCompletionRatio();

        // 2) Reputation factor from PlayerProgress (0..1)
        // Assumes cafeReputation.Value exists and is 0..1000-ish as you planned.
        float rep01 = 0f;
        if (playerProgress.cafeReputation != null)
            rep01 = Mathf.Clamp01(playerProgress.cafeReputation.Value / 1000f);

        // 3) XP earned (simple formula)
        // Example: up to 100 XP/day, scaled by goals and rep.
        int xpGained = Mathf.RoundToInt(goalCompletion * 100f * Mathf.Lerp(0.5f, 1.5f, rep01));

        // 4) Apply XP -> rank progression
        playerProgress.cafeRank.AddXP(xpGained);

        // 5) Unlock checks
        playerProgress.EvaluateUnlocks();

        // 6) Cleanup daily goals (they will be recreated on next day start)
        if (DailyGoalManager.Instance != null)
            DailyGoalManager.Instance.EndOfDay();

        // 7) Reset daily-only stats (served/lost/money/repDelta)
        stats.ResetForNewDay();

        // 8) Pause (EOD Summary UI will show on top)
        Time.timeScale = 0f;

        Debug.Log($"[EOD] goals={goalCompletion:P0}, rep01={rep01:0.00}, xp={xpGained}");

        var report = new EndOfDayReport
        {
            customersServed = stats.CustomersServed,
            customersLost = stats.CustomersLost,
            averageMood = stats.AverageMood,
            moneyEarned = stats.MoneyEarnedToday,
            goalCompletion01 = goalCompletion,
            xpGained = xpGained,
            reputationDelta = stats.ReputationDeltaToday
        };

        LastReport = report;

        // Show UI
        FindFirstObjectByType<EndOfDaySummaryUI>()?.Show(report);
    }
}

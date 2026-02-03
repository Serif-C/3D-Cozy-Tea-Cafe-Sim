using UnityEngine;

public class CafeProgressionController : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private DailyGoalManager goalManager;

    [SerializeField] private int baseXpPerDay = 100;

    public void OnDayEnded()
    {
        float completionRatio = goalManager.GetCompletionRatio();
        int xpGained = Mathf.RoundToInt(baseXpPerDay * completionRatio);

        bool leveledUp = progress.cafeRank.AddXP(xpGained);

        progress.EvaluateUnlocks();

        Debug.Log($"Day Ended: +{xpGained} XP ({completionRatio:P0})");

        if (leveledUp)
            Debug.Log($"Cafe Rank Up! Now Level {progress.cafeRank.Level}");
    }
}

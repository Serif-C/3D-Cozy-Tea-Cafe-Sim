using TMPro;
using UnityEngine;

public class CafeStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text reputationText;

    private void Update()
    {
        rankText.text =
            $"Cafe Rank: {progress.cafeRank.Level} " +
            $"({progress.cafeRank.CurrentXP}/{progress.cafeRank.XPForNextLevel})";

        reputationText.text =
            $"Reputation: {progress.cafeReputation.Value} / 1000";
    }
}

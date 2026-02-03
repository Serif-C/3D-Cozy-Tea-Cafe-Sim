using TMPro;
using UnityEngine;

public class DailyGoalRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private DailyGoal goal;

    public void Bind(DailyGoal goal)
    {
        this.goal = goal;
        Refresh();
    }

    private void Update()
    {
        if (goal != null)
            Refresh();
    }

    private void Refresh()
    {
        string check = goal.IsCompleted ? "Done" : "•";
        text.text = $"{check} {goal.Description} ({goal.Current}/{goal.Target})";
    }
}

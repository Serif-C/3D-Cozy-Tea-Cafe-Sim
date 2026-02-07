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
        if (!goal.IsAPersistenGoal)
        {
            string check = goal.IsCompleted ? "DONE" : "•";
            text.text = $"{check} {goal.Description} ({goal.Current}/{goal.Target})";
        }
        else
        {
            // Persistent goals are not be marked completed until the end of the day
            text.text = $"{goal.Description} ({goal.Current}/{goal.Target})";
        }
    }
}

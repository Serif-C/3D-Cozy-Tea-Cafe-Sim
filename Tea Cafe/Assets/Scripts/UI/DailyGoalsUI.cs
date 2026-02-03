using System.Collections;
using UnityEngine;

public class DailyGoalsUI : MonoBehaviour
{
    [SerializeField] private DailyGoalRowUI goalRowPrefab;
    [SerializeField] private Transform contentRoot;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        // Wait until the singleton exists
        while (DailyGoalManager.Instance == null)
            yield return null;

        DailyGoalManager.Instance.GoalsUpdated += BuildUI;

        // Build once in case goals already exist
        BuildUI();
        Debug.Log("DailyGoalsUI subscribed to GoalsUpdated");

    }

    private void OnDisable()
    {
        if (DailyGoalManager.Instance != null)
            DailyGoalManager.Instance.GoalsUpdated -= BuildUI;
    }

    public void BuildUI()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        var goals = DailyGoalManager.Instance.GetGoals();

        foreach (var goal in goals)
        {
            var row = Instantiate(goalRowPrefab, contentRoot);
            row.Bind(goal);
        }
    }
}

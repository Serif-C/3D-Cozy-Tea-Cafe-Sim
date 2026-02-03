using UnityEngine;

public class DailyGoalBootstrapper : MonoBehaviour
{
    [SerializeField] private CustomerServiceEventHub customerHub;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private TimeManager timeManager;

    private DailyGoalFactory factory;

    private void Start()
    {
        factory = new DailyGoalFactory(
            customerHub,
            playerManager,
            playerProgress
        );

        var goals = factory.CreateGoals(playerProgress);
        DailyGoalManager.Instance.StartNewDay(goals);

        //FindFirstObjectByType<DailyGoalsUI>()?.BuildUI();
    }
}

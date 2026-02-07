using UnityEngine;

public class DailyCafeStats : MonoBehaviour
{
    public static DailyCafeStats Instance { get; private set; }

    public int CustomersServed { get; private set; }
    public int CustomersLost { get; private set; }
    public float CustomerSatisfactionAvg { get; private set; }

    private float moodSum;
    private float moodAverage;  // Customer satisfaction/mood average
    private int moodSamples;

    public int MoneyEarnedToday { get; private set; }
    public int ReputationDeltaToday { get; private set; }

    public float AverageMood =>
        moodSamples == 0 ? 0f : moodSum / moodSamples;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.MoneyEarned += OnMoneyEarned;
    }

    // ---- EVENT HOOKS ----

    public void OnCustomerServed(float mood)
    {
        CustomersServed++;
        moodSum += mood;
        moodSamples++;
    }

    public void OnCustomerLost(float mood)
    {
        CustomersLost++;
        moodSum += mood;
        moodSamples++;
    }

    public void OnMoneyEarned(int amount)
    {
        MoneyEarnedToday += amount;
    }

    public void OnCustomerSatisfactionUpdated()
    {
        moodAverage = (moodSum / moodSamples);
    }

    public void AddReputationDelta(int delta)
    {
        ReputationDeltaToday += delta;
    }

    private void OnDestroy()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.MoneyEarned -= OnMoneyEarned;
    }

    // ---- DAY RESET ----

    public void ResetForNewDay()
    {
        CustomersServed = 0;
        CustomersLost = 0;
        moodSum = 0f;
        moodSamples = 0;
        MoneyEarnedToday = 0;
        ReputationDeltaToday = 0;
    }
}

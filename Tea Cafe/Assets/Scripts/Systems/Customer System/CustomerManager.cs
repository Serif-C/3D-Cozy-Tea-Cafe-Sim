using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Default and Current")]
    [SerializeField] private int maxNumCustomer = 10;
    [SerializeField] private int defaultMax = 10;

    [Header("Progression Settings")]
    [Tooltip("Absolute hard cap for how many customers the game can ever support")]
    [SerializeField] private int customerHardCap = 100;

    [Tooltip("Increase max customers every N ingame days")]
    [SerializeField] private int daysPerIncrease = 7;

    [Tooltip("How many extra customers to allow each step")]
    [SerializeField] private int customersPerStep = 5;

    [Header("Attributes for Tracking")]
    [SerializeField] private int daysPassed = 0;
    [SerializeField] private int currCustomerCount;

    private TimeManager timeManager;

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
        timeManager = FindFirstObjectByType<TimeManager>();
        if (timeManager != null)
        {
            timeManager.OnDayChanged += HandleDayChanged;
        }
        else
        {
            Debug.LogWarning("CustomerManager: No TimeManager found, progression will not run.");
        }
    }

    private void HandleDayChanged()
    {
        daysPassed++;

        if (daysPassed % daysPerIncrease == 0)
        {
            IncreaseMaxCustomer();
        }
    }

    public int GetMaxNumCustomer()
    {
        return maxNumCustomer;
    }

    public void IncreaseMaxCustomer()
    {
        int newMax = maxNumCustomer + customersPerStep;
        maxNumCustomer = Mathf.Min(newMax, customerHardCap);

        Debug.Log($"CustomerManager: Max customers increased to {maxNumCustomer}");
    }

    public int GetDefaultMax() { return defaultMax; }
    public int GetHardCapMax() { return customerHardCap;}
    public void IncrementCurrentCustomer() { currCustomerCount++; }
    public void DecrementCurrentCustomer() { currCustomerCount--; }
    public int GetCurrentCustomer() { return currCustomerCount; }
}

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

    private int daysPassed = 0;
    private int currCustomerCount;

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

    public int GetMaxNumCustomer()
    {
        return maxNumCustomer;
    }

    public void IncreaseMaxCustomer()
    {
        // Level-based logic goes here
    }

    public int GetDefaultMax()
    {
        return defaultMax;
    }

    public int GetCurrentCustomer()
    {
        return currCustomerCount;
    }
}

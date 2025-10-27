using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [SerializeField] private int maxNumCustomer = 10;
    private int defaultMax = 10;
    private int currCustomerCount;

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

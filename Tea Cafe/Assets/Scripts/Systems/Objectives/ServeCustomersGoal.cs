using UnityEngine;

public class ServeCustomersGoal : DailyGoal
{
    private ICustomerServedSource source;

    public ServeCustomersGoal(ICustomerServedSource source, int target)
    {
        this.source = source;
        Target = target;

        Title = "Served Customers";
        Description = $"Serve {Target} customers today";
    }

    public override void Initialize()
    {
        source.CustomerServed += OnCustomerServed;
    }

    public override void CleanUp()
    {
        source.CustomerServed -= OnCustomerServed;
    }

    private void OnCustomerServed(CustomerBrain brain)
    {
        Debug.Log("Customer served event received!");
        Increment();

        // Optional debug
        Debug.Log($"[DailyGoal] Served customer ({Current}/{Target})");
    }
}

using System;
using UnityEngine;

public class CustomerServiceEventHub : MonoBehaviour, ICustomerServedSource
{
    public event Action<CustomerBrain> CustomerServed;

    public void Register(CustomerBrain brain)
    {
        brain.CustomerServed += OnCustomerServed;
    }

    public void Unregister(CustomerBrain brain)
    {
        brain.CustomerServed -= OnCustomerServed;
    }

    private void OnCustomerServed(CustomerBrain brain)
    {
        CustomerServed?.Invoke(brain);
    }
}

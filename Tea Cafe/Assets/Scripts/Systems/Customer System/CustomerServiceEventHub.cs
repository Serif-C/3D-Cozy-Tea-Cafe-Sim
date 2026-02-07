using System;
using UnityEngine;

public class CustomerServiceEventHub : MonoBehaviour, ICustomerServedSource, ISatisfactionSource
{
    public event Action<CustomerBrain> CustomerServed;
    public event Action<float> SatisfactionUpdated;

    public void Register(CustomerBrain brain)
    {
        brain.CustomerServed += OnCustomerServed;
        brain.SatisfactionUpdated += OnCustomerSatisfactionFinalizedInternal;
    }

    public void Unregister(CustomerBrain brain)
    {
        brain.CustomerServed -= OnCustomerServed;
        brain.SatisfactionUpdated -= OnCustomerSatisfactionFinalizedInternal;
    }

    private void OnCustomerServed(CustomerBrain brain)
    {
        CustomerServed?.Invoke(brain);
    }

    private void OnCustomerSatisfactionFinalizedInternal(float satisfaction)
    {
        SatisfactionUpdated?.Invoke(satisfaction);
    }
}

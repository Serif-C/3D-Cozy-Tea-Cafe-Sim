using System;
using UnityEngine;

public interface ICustomerServedSource
{
    event Action<CustomerBrain> CustomerServed;
}

public interface IMoneyEarnedSource
{
    event System.Action<int> MoneyEarned;
}

public interface ISatisfactionSource
{
    event System.Action<float> SatisfactionUpdated;
}



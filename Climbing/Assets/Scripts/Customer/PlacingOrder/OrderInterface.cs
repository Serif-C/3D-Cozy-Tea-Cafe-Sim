using System;
using UnityEngine;

public interface IHandleOrder
{
    bool IsReady { get; }
    event Action OnReady;
}

public interface IOrderSystem
{
    IHandleOrder PlaceOrder(GameObject customer);
}

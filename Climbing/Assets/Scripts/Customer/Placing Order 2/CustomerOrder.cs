using System;
using UnityEngine;

public enum DrinkType
{
    GreenTea,
    BlackTea,
    Oolong,
    Herbal
}

// What the customer expects
public struct OrderTicket
{
    public Guid OrderId;    // unique identifier, could be a CustomerName as well
    public DrinkType Drink;
}


using System;
using UnityEngine;

// Teas and Smoothies Can be ordered anytime
public enum DrinkType // (Classic Warm Teas)
{
    // Low Complexity
    GreenTea,
    BlackTea,
    Oolong,
    Herbal,

    // Mid Complexity
    MatchaTarro,

    // High Complexity
    UberDeluxeTea
}

public enum Dessert
{
    MatchaMilkTea
}

// Breakfast, Lunch, and Dinner can only be ordered on their respective times
public enum Breakfast
{
    FruityWaffle,
    IceCreamPancake,

}

public enum Lunch
{
    GarlicSoup,
    WhaleStew
}

public enum Dinner
{
    SeaKingSteak,
    DeepFriedGiantSquid
}

public class MainDish
{
    public Breakfast breakfast;
    public Lunch lunch;
    public Dinner dinner;

    public Breakfast OrderRandomBreakfastItem()
    {
        Breakfast item = (Breakfast) UnityEngine.Random.Range(0, Enum.GetValues(typeof(Breakfast)).Length);
        breakfast = item;
        return breakfast;
    }

    public Lunch OrderRandomLunchItem()
    {
        Lunch item = (Lunch)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Lunch)).Length);
        lunch = item;
        return lunch;
    }

    public Dinner OrderRandomDinnerItem()
    {
        Dinner item = (Dinner)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Dinner)).Length);
        dinner = item;
        return dinner;
    }
}
using System;
using UnityEngine;

[System.Serializable]
public class CafeRank 
{
    public int Level = 1;
    public int CurrentXP = 0;

    public int XPForNextLevel => 100 + (Level - 1) * 50;

    public bool AddXP(int amount)
    {
        CurrentXP += amount;
        bool leveledUp = false;

        while (CurrentXP >= XPForNextLevel)
        {
            CurrentXP -= XPForNextLevel;
            Level++;
            leveledUp = true;
        }

        return leveledUp;
    }
}

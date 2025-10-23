using System.Collections.Generic;
using UnityEngine;

public enum Moods
{
    ScrewThisIAmLeaving = 0,
    VeryUnsatisfied = 20,
    Unsatisfied = 40,
    NotSatisfiedUnsatisfied = 60,
    Satisfied = 80,
    VerySatisfied = 100
}

public class CustomerMood : MonoBehaviour
{
    /*
     * 1. Goes from 0 to 100 -> 0 means customer leaves
     * 2. Influences how much tip they give to the player
     */

    public float currentMoodValue;
    private float startingMood;

    private void Awake()
    {
        // Customer starts with a mood between 30 to 70
        startingMood = Random.Range(30f, 70f);
        currentMoodValue = startingMood;
    }

    public float GetCurrentMoodValue()
    {
        return currentMoodValue;
    }

    public Moods GetMood(float moodValue)
    {
        if (moodValue <= 0)
            return Moods.ScrewThisIAmLeaving;
        else if (moodValue <= 20)
            return Moods.VeryUnsatisfied;
        else if (moodValue <= 40)
            return Moods.Unsatisfied;
        else if (moodValue <= 60)
            return Moods.NotSatisfiedUnsatisfied;
        else if (moodValue <= 80)
            return Moods.Satisfied;
        else
            return Moods.VerySatisfied;
    }

    public void DecayMood(float decayAmount)
    {
        currentMoodValue -= decayAmount;
    }
}

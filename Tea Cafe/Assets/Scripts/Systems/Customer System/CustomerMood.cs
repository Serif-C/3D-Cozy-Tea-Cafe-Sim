using System;
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
    [Range(0, 100)] public float currentMoodValue;
    [SerializeField] private Vector2 startRange = new Vector2(30f, 70f);
    public event Action<float, Moods> OnMoodChanged;
    [SerializeField] private Sprite[] emotes;
    private float startingMood;

    private void Awake()
    {
        currentMoodValue = Mathf.Clamp(UnityEngine.Random.Range(startRange.x, startRange.y), 0f, 100f);
        RaiseChanged();

        startingMood = currentMoodValue;
    }

    //public bool IsFedUp => currentMoodValue <= 0f;
    public bool IsFedUp
    {
        get { return currentMoodValue <= 0f; }
    }

    public void Decay(float amount)
    {
        SetMood(currentMoodValue - amount);
    }

    // amountPerSecond * deltaTime (caller provides dt or you can pass Time.deltaTime)
    public void DecayPerSecond(float amountPerTick, float secondsPerTick)
    {
        if (secondsPerTick <= 0f) return;
        float perSecond = amountPerTick / secondsPerTick;
        SetMood(currentMoodValue - perSecond * Time.deltaTime);
    }

    public void SetMood(float value01to100)
    {
        float clamped = Mathf.Clamp(value01to100, 0f, 100f);
        if (!Mathf.Approximately(clamped, currentMoodValue))
        {
            currentMoodValue = clamped;
            RaiseChanged();
        }
    }

    public Moods GetMood(float moodValue)
    {
        if (moodValue <= 0) return Moods.ScrewThisIAmLeaving;
        if (moodValue <= 20) return Moods.VeryUnsatisfied;
        if (moodValue <= 40) return Moods.Unsatisfied;
        if (moodValue <= 60) return Moods.NotSatisfiedUnsatisfied;
        if (moodValue <= 80) return Moods.Satisfied;
        return Moods.VerySatisfied;
    }

    private void RaiseChanged()
    {
        if (OnMoodChanged != null)
        {
            Moods moodCategory = GetMood(currentMoodValue);
            OnMoodChanged.Invoke(currentMoodValue, moodCategory);
        }
    }

    public Sprite GetEmote(float moodValue)
    {
        if (GetMood(moodValue) == Moods.ScrewThisIAmLeaving) return emotes[0];
        else if (GetMood(moodValue) == Moods.VeryUnsatisfied) return emotes[1];
        else if (GetMood(moodValue) == Moods.Unsatisfied) return emotes[2];
        else if (GetMood(moodValue) == Moods.NotSatisfiedUnsatisfied) return emotes[3];
        else if (GetMood(moodValue) == Moods.Satisfied) return emotes[4];
        else return emotes[5];
    }

    public void ResetMood()
    {
        currentMoodValue = startingMood;
    }
}

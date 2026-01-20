using System;
using UnityEngine;

public enum MealTime
{
    BreakfastTime,
    LunchTime,
    DinnerTime
}

public class TimeManager : MonoBehaviour, ITimeScaleController
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Scale")]
    [Tooltip("How many real-time seconds a full in-game day lasts")]
    [SerializeField] private float realSecondsPerGameDay = 3600f;
    [Tooltip("Value == 1.0 normal speed, 2.0 two times speed, etc....")]
    [SerializeField] private float defaultTimeScale = 1f;

    private float previousTimeScale = 1f;
    public float Current => Time.timeScale;

    [Header("Start Time")]
    [SerializeField] private int startHour = 6;
    [SerializeField] private int startMinute = 0;

    [Header("Start Date")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startMonth = 1;
    [SerializeField] private int startYear = 1;
    private int totalDaySinceStart = 0;

    // Public API
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Day { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }

    /// <summary>0..1 value representing how far we are in the current day.</summary>
    public float DayProgress01 { get; private set; }
    public Action<int, int> OnHourChanged;
    public Action OnDayChanged;

    [Header("Current Time")]
    [SerializeField] private float timeInCurrentDaySeconds;
    [SerializeField] private int currentHour = 0;
    [SerializeField] private int currentMinute = 0;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int currentMonth = 1;
    [SerializeField] private MealTime mealTime = MealTime.BreakfastTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        previousTimeScale = defaultTimeScale;
        Time.timeScale = defaultTimeScale;

        Day = startDay;
        Month = startMonth;
        Year = startYear;

        float totalMinutes = startHour * 60f + startMinute;     // Basically minutes passed since 00:00 (midnight)
        DayProgress01 = totalMinutes / (24f * 60);
        timeInCurrentDaySeconds = DayProgress01 * realSecondsPerGameDay;

        UpdateClockFields();

        currentHour = startHour;
        currentMinute = startMinute;
        mealTime = TrackMealTime();
    }

    private void Update()
    {
        timeInCurrentDaySeconds += Time.deltaTime;

        if (timeInCurrentDaySeconds >= realSecondsPerGameDay)
        {
            timeInCurrentDaySeconds -= realSecondsPerGameDay; // basically = 0
            IncrementDate();
            OnDayChanged?.Invoke();
        }

        DayProgress01 = timeInCurrentDaySeconds / realSecondsPerGameDay;

        // Calculate current time from day progress
        float totalMinutes = DayProgress01 * 24f * 60f;
        int newHour = Mathf.FloorToInt(totalMinutes / 60f);
        int newMinute = Mathf.FloorToInt(totalMinutes % 60f);

        // only fire event when hour changes (to avoid spam)
        if (newHour != Hour)
        {
            Hour = newHour;
            currentHour = Hour;
            Minute = newMinute;
            OnHourChanged?.Invoke(Hour, Minute);
            mealTime = TrackMealTime();
        }
        else
        {
            Minute = newMinute;
            currentMinute = Minute;
        }
    }

    private void UpdateClockFields()
    {
        float totalMinutes = DayProgress01 * 24f * 60f;
        Hour = Mathf.FloorToInt(totalMinutes / 60f);
        Minute = Mathf.FloorToInt(totalMinutes % 60f);
    }

    private void IncrementDate()
    {
        Day++;
        totalDaySinceStart++;
        currentDay = Day;
        int daysInMonth = GetDaysInMonth(Month);

        if (Day > daysInMonth)
        {
            Day = 1;
            Month++;
            currentMonth = Month;

            if (Month > 12) { Month = 1; Year++; }
        }
    }

    public int GetDayOfWeekIndex(int startDayOfWeek = 0)
    {
        return (startDayOfWeek + totalDaySinceStart) % 7;
    }

    private int GetDaysInMonth(int month)
    {
        if (month == 2) return 28;
        if (month == 4 || month == 6 || month == 9 || month == 11) return 30;
        return 31;
    }

    private MealTime TrackMealTime()
    {
        if (currentHour >= 4 && currentHour <= 12) return MealTime.BreakfastTime;
        else if (currentHour > 12 && currentHour <= 19) return MealTime.LunchTime;
        else return MealTime.DinnerTime;
    }

    public MealTime GetMealTime()
    {
        return mealTime;
    }

    // ----------------------------
    // Interface Implementation
    // ----------------------------

    public void Pause()
    {
        // Save the current timeScale so Resume() restores it
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        // Restore the previous time scale
        Time.timeScale = previousTimeScale <= 0f ? defaultTimeScale : previousTimeScale;
    }

    public void Set(float scale)
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = Mathf.Max(0f, scale);
    }

    public void SetTimeScale(float newTimeScale)
    {
        Set(newTimeScale);
    }

    public void RevertTimeScale()
    {
        Resume();
    }
}

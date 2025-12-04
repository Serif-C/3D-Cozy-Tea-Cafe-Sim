using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Time Scale")]
    [Tooltip("How many real-time seconds a full in-game day lasts")]
    [SerializeField] private float realSecondsPerGameDay = 3600f;

    [Header("Start Time")]
    [SerializeField] private int startHour = 6;
    [SerializeField] private int startMinute = 0;

    [Header("Start Date")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startMonth = 1;
    [SerializeField] private int startyear = 1;

    // Public API
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Day { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }

    /// <summary>0..1 value representing how far we are in the current day.</summary>
    public float DayProgress01 { get; private set; }
}

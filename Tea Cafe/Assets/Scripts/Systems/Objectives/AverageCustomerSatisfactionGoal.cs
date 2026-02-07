using System.Collections.Generic;
using UnityEngine;

public class AverageCustomerSatisfactionGoal : DailyGoal
{
    private ISatisfactionSource satisfactionSource;
    private readonly List<float> samples = new();

    public AverageCustomerSatisfactionGoal(ISatisfactionSource source, int target)
    {
        base.MakeGoalTrackingPersisten(true); // Tracks AVG satisfaction throughout the day

        this.satisfactionSource = source;
        Target = target;

        Title = "Average Customer Satisfaction";
        Description = $"Have a %{Target} Satisfaction";
    }

    public override void Initialize()
    {
        satisfactionSource.SatisfactionUpdated += OnSatisfaction;
    }

    public override void CleanUp()
    {
        satisfactionSource.SatisfactionUpdated -= OnSatisfaction;
    }

    public void OnSatisfaction(float value)
    {
        samples.Add(value);

        float average = CalculateAverage();
        //SetProgress(average);
        Debug.Log($"[DailyGoal] Served customer average mood ({Current}/{Target})");
    }

    public float CalculateAverage()
    {
        if (samples.Count == 0)
            return 0f;

        float sum = 0f;
        foreach (var v in samples)
            sum += v;

        Current = (int) sum;
        return sum / samples.Count;
    }
}

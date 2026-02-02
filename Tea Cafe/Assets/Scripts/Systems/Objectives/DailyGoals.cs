using UnityEngine;

public abstract class DailyGoal
{
    public string Title { get; protected set; }
    public string Description { get; protected set; }

    public int Target { get; protected set; }
    public int Current {  get; protected set; }

    public bool IsCompleted => Current >= Target;

    public virtual float Progress01 => (float)Current / Target;

    public abstract void Initialize();
    public abstract void CleanUp();

    public void Increment(int amount = 1)
    {
        Current += amount;
        Current = Mathf.Min(Current, Target);
    }
}

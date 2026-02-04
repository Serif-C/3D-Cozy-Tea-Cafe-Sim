using UnityEngine;

[System.Serializable]
public class CafeReputation
{ 
    [Range(0, 1000)]
    public int Value = 0;

    public void Add(int amount)
    {
        Value = Mathf.Clamp(Value + amount, 0, 1000);
    }
}

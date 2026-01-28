using System.Collections.Generic;
using UnityEngine;

public class LoungeManager : MonoBehaviour
{
    public static LoungeManager Instance { get; private set; }

    private readonly List<LoungeSeat> seats = new();
    private readonly HashSet<LoungeSeat> reserved = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Refresh();
    }

    public void Refresh()
    {
        seats.Clear();

        var gos = GameObject.FindGameObjectsWithTag("LoungeSeat");
        foreach (var go in gos)
        {
            if (go != null && go.TryGetComponent(out LoungeSeat seat))
                seats.Add(seat);
        }
    }

    public bool TryReserveAny(out LoungeSeat seat)
    {
        seat = null;
        for (int i = 0; i < seats.Count; i++)
        {
            var s = seats[i];
            if (s == null) continue;
            if (reserved.Contains(s)) continue;

            reserved.Add(s);
            seat = s;
            return true;
        }
        return false;
    }

    public void Release(LoungeSeat seat)
    {
        if (seat == null) return;
        reserved.Remove(seat);
    }
}

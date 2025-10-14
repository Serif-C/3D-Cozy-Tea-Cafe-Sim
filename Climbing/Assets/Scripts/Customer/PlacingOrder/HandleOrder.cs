using System;
using UnityEngine;

public class HandleOrder : MonoBehaviour, IHandleOrder
{
    public bool IsReady { get; private set; }

    public event Action OnReady;

    public void MarkReady()
    {
        if (IsReady) return;
        IsReady = true;
        
        // if (OnReady != null) {OnReady();}
        OnReady?.Invoke();
    }
}

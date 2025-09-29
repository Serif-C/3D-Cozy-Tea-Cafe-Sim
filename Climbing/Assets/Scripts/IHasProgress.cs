using System;
using UnityEngine;

public interface IHasProgress
{
    // Normalized progress 0...1 (0 = just started, 1 = done)
    float Progress01 { get; }

    // Should the bar be visible right now? (e.g., only while active)
    bool ShowProgress { get; }

    // Notify listeners when progress updates (optional but efficient)
    event Action<float, bool> OnProgressChanged;
}

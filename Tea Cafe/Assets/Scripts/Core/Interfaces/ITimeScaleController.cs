using UnityEngine;

public interface ITimeScaleController
{
    void Pause();
    void Resume();
    void Set(float scale);
    float Current { get; }
}

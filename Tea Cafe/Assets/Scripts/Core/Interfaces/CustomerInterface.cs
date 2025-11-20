using UnityEngine;
using System;

public interface ITarget
{
    Vector3 Position { get; }
}

public interface IMover
{
    bool IsMoving { get; }
    event Action ReachedTarget;
    void GoTo(ITarget target);
    void Stop();
    void Warp(Vector3 position);
}

public interface IResettable
{
    public void ResetObject();
}
using UnityEngine;

public class TransformTarget : MonoBehaviour, ITarget
{
    public Vector3 Position => transform.position;
}

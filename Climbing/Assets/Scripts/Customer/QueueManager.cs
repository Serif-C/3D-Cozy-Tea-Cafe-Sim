using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] spots;
    int next;
    public ITarget RequestSpot()
    {
        int i = next;
        next = next + 1;
        int clamped = Mathf.Min(i, spots.Length - 1);
        return spots[clamped];
    }
}

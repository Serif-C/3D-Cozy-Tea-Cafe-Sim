using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] spots;
    int next;

    private void Awake()
    {
        var tag = GameObject.FindGameObjectsWithTag("Queue");
        var list = new List<TransformTarget>(tag.Length);

        foreach (var q in tag)
        {
            if (q.TryGetComponent(out TransformTarget queue))
            {
                list.Add(queue);
            }
        }

        spots = list.ToArray();
    }

    public ITarget RequestSpot()
    {
        int i = next;
        next = next + 1;
        int clamped = Mathf.Min(i, spots.Length - 1);
        return spots[clamped];
    }
}

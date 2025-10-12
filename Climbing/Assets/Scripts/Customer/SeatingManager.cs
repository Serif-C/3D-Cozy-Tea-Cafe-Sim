using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SeatingManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] seats;

    private void Awake()
    {
        var tag = GameObject.FindGameObjectsWithTag("Seats");
        var list = new List<TransformTarget>(tag.Length);

        foreach (var go in tag)
        {
            if (go.TryGetComponent(out TransformTarget seat)) list.Add(seat);
        }

        seats = list.ToArray();
    }

    public ITarget AssignSeat()
    {
        int random = UnityEngine.Random.Range(0, seats.Length);
        return seats[random];
    }
}

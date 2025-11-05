using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Search;
using UnityEngine;

public class SeatingManager : MonoBehaviour
{
    public static SeatingManager Instance { get; private set; }
    //[SerializeField] private TransformTarget[] seats;
    [SerializeField] private List<TransformTarget> seats;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var tag = GameObject.FindGameObjectsWithTag("Seats");
        var list = new List<TransformTarget>(tag.Length);
        //var listOfTables = new List<Table>();

        foreach (var s in tag)
        {
            if (s.TryGetComponent(out TransformTarget seat))
            {
                list.Add(seat);
            }
        }

        seats = list;
    }

    public bool TryReserveRandomFreeSeat(out TransformTarget seat)
    {
        seat = null;

        // Build a list of free seats
        var free = new List<int>(seats.Count);

        for (int i = 0; i < seats.Count; i++)
        {
            var table = seats[i].GetComponentInParent<Table>();
            if (table != null && !table.IsTableOccupied())
            { 
                free.Add(i); 
            }
        }

        if (free.Count == 0)
            return false;   // if nothing is free, let caller wait

        // Pick one at random
        int pick = free[Random.Range(0, free.Count)];
        var pickedSeat = seats[pick];

        // Reserve immidiately to prevent customers from racing to it
        pickedSeat.GetComponentInParent<Table>().SetOccupiedValue(true);

        seat = pickedSeat;
        return true;

    }

    public void AddSeat(Transform seat)
    {
        seats.Add(seat.gameObject.GetComponent<TransformTarget>());
    }

    public void RemoveSeat(Transform seat)
    {
        seats.Remove(seat.gameObject.GetComponent<TransformTarget>());
    }
}

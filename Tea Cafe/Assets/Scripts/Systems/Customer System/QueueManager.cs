using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] spots;   // ordered: [0] nearest counter
    private TransformTarget counter;
    private CustomerBrain atCounter;
    public ITarget CounterTarget => counter;

    // After building and assigning spots
    private bool[] occupied;
    private readonly List<CustomerBrain> line = new();  // current queue, 0 = front
    private readonly Dictionary<CustomerBrain, int> slotByCustomer = new();

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

        counter = GameObject.FindGameObjectWithTag("Counter").gameObject.GetComponent<TransformTarget>();

        // If need a strict order, sort here by distance to counter (or keep Inspector order)
        list.Sort((a, b) => Vector3.Distance(counter.GetComponent<Transform>().position, a.transform.position)
                        .CompareTo(Vector3.Distance(counter.GetComponent<Transform>().position, b.transform.position)));

        spots = list.ToArray();

        occupied = new bool[spots.Length];
    }

    private void Reserve(int index, CustomerBrain customer)
    {
        occupied[index] = true;
        slotByCustomer[customer] = index;
        customer.UpdateQueueTarget(spots[index]);   // tell customer where to stand
    }

    private void Free(CustomerBrain customer)
    {
        if (slotByCustomer.TryGetValue(customer, out var idx))
        {
            occupied[idx] = false;
            slotByCustomer.Remove(customer);
        }
    }

    private int FindFreeSlotStartingAt(int start)
    {
        for (int i = start; i < spots.Length; i++) if (!occupied[i]) return i;
        for (int i = 0; i < occupied.Length; i++) if (!occupied[i]) return i;
        return spots.Length - 1;    // clamp overflow to the back
    }

    // Customer joins the queue; return the spot they should go to now
    public ITarget Join(CustomerBrain customer)
    {
        if (!line.Contains(customer))
        {
            line.Add(customer);
        }

        int desired = Mathf.Clamp(line.IndexOf(customer), 0, spots.Length - 1);
        int idx = FindFreeSlotStartingAt(desired);
        Reserve(idx, customer);

        ReAssignAll(); // compress everyone right away

        return spots[idx];
    }

    public void Leave(CustomerBrain customer)
    {
        int i = line.IndexOf(customer);
        if (i < 0) return;
        line.RemoveAt(i);
        Free(customer);
        ReAssignAll(); // shift everyone forward
    }

    public bool IsMyTurn(CustomerBrain customer)
    {
        return line.Count > 0 && line[0] == customer;   // is this customer in front of the line
    }

    public void NotifyCustomerAdvanced(CustomerBrain customer)
    {
        // tell this customer to move to their new spot
        int index = IndexOf(customer);
        if (index < 0) return;
        customer.UpdateQueueTarget(GetSpotForIndex(index));
    }

    public void ReAssignAll()
    {
        System.Array.Fill(occupied, false);
        slotByCustomer.Clear();

        for (int i = 0; i < line.Count; i++)
        {
            var c = line[i];
            int desired = Mathf.Clamp(i, 0, spots.Length - 1);
            int idx = FindFreeSlotStartingAt(desired);
            Reserve(idx, c);
        }
    }

    private ITarget GetSpotForIndex(int index)
    {
        if (spots == null || spots.Length == 0)
        {
            return null;
        }

        // clamp is used to account for unexpected edge cases
        // otherwise, it can simply be" ` return spots[index]; `
        int clamped = Mathf.Clamp(index, 0, spots.Length - 1);
        return spots[clamped];
    }


    public bool TryAcquireCounter(CustomerBrain customer)
    {
        if (IsMyTurn(customer) && (atCounter == null || atCounter == customer))
        {
            atCounter = customer;
            Leave(customer);    // peel off front; re-assign all
            return true;
        }

        return false;
    }

    public void ReleaseCounter(CustomerBrain customer)
    {
        if (atCounter == customer) atCounter = null;
    }

    private int IndexOf(CustomerBrain customer)
    {
        return line.IndexOf(customer);
    }
}

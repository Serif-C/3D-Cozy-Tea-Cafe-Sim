using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] spots;   // ordered: [0] nearest counter
    private readonly List<CustomerBrain> line = new();  // current queue, 0 = front
    private TransformTarget counter;
    private CustomerBrain atCounter;
    public ITarget CounterTarget => counter;

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
    }

    // Customer joins the queue; return the spot they should go to now
    public ITarget Join(CustomerBrain customer)
    {
        if (!line.Contains(customer))
        {
            line.Add(customer);
            ReAssignAll(); // compress everyone right away
        }

        return GetSpotForIndex(IndexOf(customer));
    }

    public void Leave(CustomerBrain customer)
    {
        int index = IndexOf(customer);
        if (index < 0) return;

        line.RemoveAt(index);
        ReAssignAll();
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
        for (int i = 0; i < line.Count; i++)
        {
            line[i].UpdateQueueTarget(GetSpotForIndex(i));
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
            //Leave(customer);    // peel off front; re-assign all
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

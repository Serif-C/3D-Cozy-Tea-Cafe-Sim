using UnityEngine;
using System;
using static UnityEngine.CullingGroup;
using System.Collections;

public enum CustomerState
{
    EnteringCafe,
    WaitingInLine,
    PlacingOrder,
    Sitting,
    Drinking,
    LeavingCafe
}

public class CustomerBrain : MonoBehaviour
{
    [SerializeField] private MonoBehaviour moverProvider;

    private IMover mover;

    [SerializeField] private TransformTarget entry;
    [SerializeField] private TransformTarget counter;
    [SerializeField] private TransformTarget exit;
    [SerializeField] private QueueManager queue;
    [SerializeField] private SeatingManager seating;

    public CustomerState current { get; private set; }
    public event Action<CustomerState> OnStateChanged;

    private void Awake()
    {
        mover = (IMover)moverProvider;
    }

    private void Start()
    {
        StartCoroutine(Run());
    }

    private void SetState(CustomerState s)
    {
        current = s;
        //Debug.Log("Customer - Current State: " + current.ToString());
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(s);
        }
    }

    IEnumerator Run()
    {
        yield return EnterCafe();
        yield return WaitInLine();
        yield return PlaceOrder();
        yield return SitAndDrink();
        yield return LeaveCafe();
    }

    IEnumerator EnterCafe()
    {
        SetState(CustomerState.EnteringCafe);
        yield return Go(entry);
    }

    IEnumerator WaitInLine()
    {
        SetState(CustomerState.WaitingInLine);
        var spot = queue.RequestSpot();
        yield return Go(spot);
        // ... “turn in line” logic here ...
    }

    IEnumerator PlaceOrder()
    {
        SetState(CustomerState.PlacingOrder);
        yield return Go(counter);
        // ... order logic ...
    }

    IEnumerator SitAndDrink()
    {
        SetState(CustomerState.Sitting);
        var seat = seating.AssignSeat();
        yield return Go(seat);
        SetState(CustomerState.Drinking);
        yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 8f));
    }

    IEnumerator LeaveCafe()
    {
        SetState(CustomerState.LeavingCafe);
        yield return Go(exit);
    }



    /* Create a flag done = false;
     * Define a tiny helper function Handler() that sets done = true
     * Tell movement to start going somewhere
     * Subscribe to the movement’s event — “when you arrive, call Handler()”
     * Wait (do nothing) until done flips to true
     * Once we’re done waiting, unsubscribe from the event (cleanup)
     */
    IEnumerator Go(ITarget t)
    {
        bool done = false;
        void Handler()
        {
            done = true; 
        }
        mover.ReachedTarget += Handler;
        mover.GoTo(t);

        //  Pause the coroutine here and wait until 'done' becomes true.
        //  The lambda () => done creates a function that returns 'done'.
        //  Unity will resume this coroutine only when that function returns true,
        //  meaning the mover has reached its target.
        yield return new WaitUntil(() => done);

        mover.ReachedTarget -= Handler;
    }
}

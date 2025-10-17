using UnityEngine;
using System;
using static UnityEngine.CullingGroup;
using System.Collections;
using System.Net.Sockets;

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
    [Header("Customer Action Providers")]
    [SerializeField] private MonoBehaviour moverProvider;
    [SerializeField] private OrderSystem orderSystemProvider;
    private IMover mover;
    private IOrderSystem orderSystem;

    [Header("Customer Destination References")]
    [SerializeField] private TransformTarget entry;
    [SerializeField] private TransformTarget counter;
    [SerializeField] private TransformTarget exit;
    [SerializeField] private QueueManager queue;
    [SerializeField] private SeatingManager seating;

    public CustomerState current { get; private set; }
    public event Action<CustomerState> OnStateChanged;

    [Header("Customer Order Settings")]
    private OrderTicket ticket;
    private DrinkType PickDrinkForThisCustomer() => DrinkType.Herbal;

    private void Awake()
    {
        mover = (IMover)moverProvider;
        orderSystem = (IOrderSystem)orderSystemProvider;

        entry = GameObject.FindGameObjectWithTag("Entrance").gameObject.GetComponent<TransformTarget>();
        counter = GameObject.FindGameObjectWithTag("Counter").gameObject.GetComponent<TransformTarget>();
        exit = GameObject.FindGameObjectWithTag("Exit").gameObject.GetComponent<TransformTarget>();
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
        // Create the customer's ticket (no need to wait for anything here)
        ticket = new OrderTicket
        {
            OrderId = Guid.NewGuid(),
            Drink = PickDrinkForThisCustomer()
        };

        //var handle = orderSystem.PlaceOrder(gameObject);

        //// Wait (do nothing) until the order is ready, via event
        //bool rdy = false;
        //void MarkReady() { rdy = true; };

        //handle.OnReady += MarkReady;
        //yield return new WaitUntil(() => rdy);
        //handle.OnReady -= MarkReady;
    }

    IEnumerator SitAndDrink()
    {
        SetState(CustomerState.Sitting);

        // SeatingManager returns an ITarget that is actually a TransformTarget on the seat
        var seatTarget = seating.AssignSeat();      // returns ITarget, but it's a TransformTarget internally
        var seatTT = (TransformTarget)seatTarget;   // safe downcast 
        var table = seatTT.GetComponentInParent<Table>();   // find the Table for this seat
        yield return Go(seatTarget);    // Walk to seat

        // Now wait until the correct drink shows up on THIS table
        while (table == null || !table.HasDrinkFor(ticket))
            yield return new WaitForSeconds(0.25f);

        // Got the right drink -> start drinking
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

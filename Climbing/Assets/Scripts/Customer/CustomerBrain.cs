using UnityEngine;
using System;
using static UnityEngine.CullingGroup;
using System.Collections;
using System.Net.Sockets;
using NUnit.Framework;

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
    public CustomerState stateRightNow;
    public event Action<CustomerState> OnStateChanged;

    [Header("Customer Order Settings")]
    private DrinkType desiredDrink = DrinkType.BlackTea;
    //private DrinkType PickDrinkForThisCustomer() => DrinkType.BlackTea;
    [SerializeField] private int sizeOfMenu;    // The number of drinks in DrinkType enum

    // For parenting/unparenting the customer
    private Transform originalParent;
    private Table currentTable;

    // For Queue spots
    private ITarget currentQueueSpot;
    private Coroutine queueMove;

    // For CustomerSpawner 
    private System.Action<GameObject> releaseToPool;
    public void Init(System.Action<GameObject> releasePool) { releaseToPool = releasePool; }
    public void DeSpawn() { releaseToPool?.Invoke(gameObject); }


    private void Awake()
    {
        originalParent = transform.parent;
        mover = (IMover)moverProvider;
        orderSystem = (IOrderSystem)orderSystemProvider;

        entry = GameObject.FindGameObjectWithTag("Entrance").gameObject.GetComponent<TransformTarget>();
        counter = GameObject.FindGameObjectWithTag("Counter").gameObject.GetComponent<TransformTarget>();
        exit = GameObject.FindGameObjectWithTag("Exit").gameObject.GetComponent<TransformTarget>();

        if (queue == null)
            queue = FindFirstObjectByType<QueueManager>(); // single shared instance

        foreach (DrinkType drink in Enum.GetValues(typeof(DrinkType)))
        {
            sizeOfMenu++;
        }
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

        currentQueueSpot = queue.Join(this);
        if (currentQueueSpot != null)
            yield return Go(currentQueueSpot);

        // Wait until customer is in front AND the customer is free
        while (true)
        {
            if (queue.IsMyTurn(this))
            {
                if (queue.TryAcquireCounter(this))
                    break; // this customer owns the counter now
            }

            yield return new WaitForSeconds(0.1f);
        }

        // Since this customer now owns the counter:
        // Leave the line (compress others) then walk to the counter
        //queue.Leave(this);
        yield return Go(queue.CounterTarget);
    }

    IEnumerator PlaceOrder()
    {
        SetState(CustomerState.PlacingOrder);

        // ... order logic ...
        // Ordering time for now to simulate ordering
        desiredDrink = OrderForARandomDrink();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));

        // free the counter for the next customer
        queue.ReleaseCounter(this);
    }

    IEnumerator SitAndDrink()
    {
        TransformTarget seatTarget = null;

        // NEW: just retry seat reservation on a short cadence (no queue churn)
        while (!seating.TryReserveRandomFreeSeat(out seatTarget))
        {
            yield return new WaitForSeconds(0.5f);
        }

        SetState(CustomerState.Sitting);

        var seatTT = seatTarget;   // safe downcast 
        var table = seatTT.GetComponentInParent<Table>();   // find the Table for this seat

        if(table == null) { Debug.LogError("Table is null"); }
        

        yield return Go(seatTarget);    // Walk to seat
        // Attach to table for sitting/drinking
        AttachToTable(table);

        // Wait until the correct drink shows up on THIS table
        while (currentTable == null || !currentTable.HasDrinkOfType(desiredDrink))
            yield return new WaitForSeconds(0.25f);

        SetState(CustomerState.Drinking);
        yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 8f));

    }

    private void AttachToTable(Table table)
    {
        currentTable = table;


        // Keep world position/rotation as-is when parenting (important!)
        // If your Table has non-uniform scale, consider leaving customers unparented,
        // or ensure the Table and its ancestors use uniform scale (1,1,1).
        transform.SetParent(table.transform, worldPositionStays: true);
    }

    private void DetachToTable()
    {
        transform.SetParent(originalParent, worldPositionStays: true);

        if (currentTable != null)
        {
            currentTable.SetOccupiedValue(false);   // mark table free
            currentTable = null;
        }
    }

    IEnumerator LeaveCafe()
    {
        SetState(CustomerState.LeavingCafe);

        DetachToTable();

        yield return Go(exit);

        DeSpawn();
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

    public void UpdateQueueTarget(ITarget newSpot)
    {
        currentQueueSpot = newSpot;

        if (current == CustomerState.WaitingInLine && newSpot != null)
        {
            // Re-issue movement to the new spot
            if (queueMove != null) StopCoroutine(queueMove);
            queueMove = StartCoroutine(Go(newSpot));
        }
    }

    private void Update()
    {
        // Just to see in the inspector the current state
        stateRightNow = current;

        ChangeColor();
    }

    // For Testing Purposes
    [SerializeField] private Material[] customerMaterials;
    private void ChangeColor()
    {
        if(stateRightNow.Equals(CustomerState.Drinking))
        {
            gameObject.GetComponent<MeshRenderer>().material = customerMaterials[1];
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = customerMaterials[0];
        }
    }

    private DrinkType OrderForARandomDrink()
    {
        DrinkType randomDrinkType = (DrinkType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(DrinkType)).Length);
        return randomDrinkType;
    }
}

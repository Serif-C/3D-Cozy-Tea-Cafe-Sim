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
    public CustomerState stateRightNow;
    public event Action<CustomerState> OnStateChanged;

    [Header("Customer Order Settings")]
    private DrinkType desiredDrink = DrinkType.BlackTea;
    //private DrinkType PickDrinkForThisCustomer() => DrinkType.BlackTea;

    // For parenting/unparenting the customer
    private Transform originalParent;
    private Table currentTable;

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
        //desiredDrink = PickDrinkForThisCustomer();
    }

    IEnumerator SitAndDrink()
    {
        SetState(CustomerState.Sitting);

        // SeatingManager returns an ITarget that is actually a TransformTarget on the seat
        var seatTarget = seating.AssignSeat();      // returns ITarget, but it's a TransformTarget internally
        var seatTT = (TransformTarget)seatTarget;   // safe downcast 
        var table = seatTT.GetComponentInParent<Table>();   // find the Table for this seat

        if(table == null) { Debug.LogError("Table is null"); }
        yield return Go(seatTarget);    // Walk to seat

        // Attach to table for sitting/drinking
        AttachToTable(table);

        //// Now wait until the correct drink shows up on THIS table
        //while (table == null || !table.HasDrinkOfType(desiredDrink))
        //    yield return new WaitForSeconds(0.25f);

        // Wait until the correct drink shows up on THIS table
        while (currentTable == null || !currentTable.HasDrinkOfType(desiredDrink))
            yield return new WaitForSeconds(0.25f);

        // Got the right drink -> start drinking
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
        //transform.localScale = new Vector3(1/3, 1, 1/3);

    }

    private void DetachToTable()
    {
        transform.SetParent(originalParent, worldPositionStays: true);
        currentTable = null;
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
}

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
    private IMover mover;

    [Header("Customer Destination References")]
    [SerializeField] private TransformTarget entry;
    [SerializeField] private TransformTarget counter;
    [SerializeField] private TransformTarget exit;
    [SerializeField] private QueueManager queue;
    private SeatingManager seating;

    public CustomerState current { get; private set; }
    public CustomerState stateRightNow;
    public event Action<CustomerState> OnStateChanged;

    [Header("Customer Order Settings")]
    [SerializeField] private DrinkType desiredDrink = DrinkType.BlackTea;
    //private DrinkType PickDrinkForThisCustomer() => DrinkType.BlackTea;
    [SerializeField] private int sizeOfMenu;    // The number of drinks in DrinkType enum
    private CustomerMood myMood;
    [SerializeField] private float moodDecayAmount = 2f;    // The amount of mood deducted
    [SerializeField] private float moodDecayRate = 0.25f;   // The frequency the is deducted

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

    [Header("Coin and Tip Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float minTip = 0f;
    [SerializeField] private float maxTip = 20f;
    // Animation curve tipping: Most tip low, but rarely, very happy customers tip really high
    [SerializeField] private AnimationCurve tipByMood = AnimationCurve.Linear(0f, 0f, 1f, 1f); 


    private void Awake()
    {
        originalParent = transform.parent;
        mover = (IMover)moverProvider;

        entry = GameObject.FindGameObjectWithTag("Entrance").gameObject.GetComponent<TransformTarget>();
        counter = GameObject.FindGameObjectWithTag("Counter").gameObject.GetComponent<TransformTarget>();
        exit = GameObject.FindGameObjectWithTag("Exit").gameObject.GetComponent<TransformTarget>();

        if (queue == null)
            queue = FindFirstObjectByType<QueueManager>(); // single shared instance

        // Resolve seating if not set
        if (seating == null)
            seating = SeatingManager.Instance != null
                ? SeatingManager.Instance
                : FindFirstObjectByType<SeatingManager>();

        // Safety log
        if (seating == null)
            Debug.LogError("CustomerBrain: SeatingManager not found in scene.");

        foreach (DrinkType drink in Enum.GetValues(typeof(DrinkType)))
        {
            sizeOfMenu++;
        }

        myMood = gameObject.GetComponent<CustomerMood>();
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
        {
            //myMood.DecayMood(moodDecayAmount);
            // time-scaled decay: amountPerSecond * deltaTime
            //if (myMood.GetCurrentMoodValue() <= 0) break;
            //yield return new WaitForSeconds(moodDecayRate);

            myMood.DecayPerSecond(moodDecayAmount, moodDecayRate);

            if (myMood.IsFedUp)
            {
                SetState(CustomerState.LeavingCafe);
                // play here: angry animation or something
                yield return LeaveCafe();
                yield break;
            }

            yield return null;

        }

        SetState(CustomerState.Drinking);
        yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 8f));

        // Customer Successfully finished drinking -> give coin to the player
        int tipAmount = ComputeTipFromMood(myMood.currentMoodValue);
        if (tipAmount > 0)
        {
            GameObject coin = Instantiate(coinPrefab, table.GetSpawnPoint().position, Quaternion.identity);
            Coin coinComp = coin.GetComponent<Coin>();
            if (coinComp != null)
            {
                coinComp.SetAmountWithTip(tipAmount);
            }
        }

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


    private DrinkType OrderForARandomDrink()
    {
        DrinkType randomDrinkType = (DrinkType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(DrinkType)).Length);
        return randomDrinkType;
    }

    private int ComputeTipFromMood(float mood01to100)
    {
        // Example: k = (59 / 100) = 0.59f, 
        //          raw = 0 + (20 - 0) * k = 11.8f
        float k = Mathf.Clamp01(mood01to100 / 100f);
        float raw = Mathf.Lerp(minTip, maxTip, k);
        //float curve = Mathf.Clamp01(tipByMood.Evaluate(k));

        // Additional randomness to make it feel natural
        float jitter = UnityEngine.Random.Range(-k, k);
        int final = Mathf.Max(0, Mathf.RoundToInt(raw + jitter));
        return final;
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

using UnityEngine;
using System;
using static UnityEngine.CullingGroup;
using System.Collections;
using System.Net.Sockets;
using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;

public enum CustomerState
{
    EnteringCafe,
    WaitingInLine,
    PlacingOrder,
    Sitting,
    Drinking,
    LeavingCafe,
    Browsing,    // Customer walks around viewing decorations  
}

public class CustomerBrain : MonoBehaviour, IResettable, ICustomerServedSource
{
    private TimeManager timeManager;

    [Header("Customer Action Providers")]
    [SerializeField] private MonoBehaviour moverProvider;
    private IMover mover;

    [Header("Customer Destination References")]
    [SerializeField] private TransformTarget entry;
    [SerializeField] private TransformTarget counter;
    [SerializeField] private TransformTarget exit;
    [SerializeField] private float exitRadius = 1.5f;
    [SerializeField] private QueueManager queue;
    [SerializeField] private DecorationManager decor;
    [SerializeField] private float decorViewRadius = 1.5f;
    [SerializeField] private bool interestedInWaiting;
    private TransformTarget decorationViewSpot;
    private SeatingManager seating;
    private TransformTarget mySeat;

    [Header("UI Related")]
    [SerializeField] private OrderBubble orderBubble;
    [SerializeField] private SpriteRenderer emote;

    [Header("Customer State")]
    public CustomerState current { get; private set; }
    public CustomerState stateRightNow;
    public event Action<CustomerState> OnStateChanged;

    [Header("Customer Order Settings")]
    [SerializeField] private DrinkType desiredDrink = DrinkType.BlackTea;
    [SerializeField] private MainDish desiredDish;
    public event Action<CustomerBrain> CustomerServed;

    [Header("Customer Mood Settings")]
    private CustomerMood myMood;
    [SerializeField] private float moodDecayAmount = 2f;    // The amount of mood deducted
    [SerializeField] private float moodDecayRate = 0.25f;   // The frequency the is deducted

    [Header("Order Progression")]
    [SerializeField] private int breakfastUnlockDay = 5;
    [SerializeField] private int lunchUnlockDay = 7;
    [SerializeField] private int dinnerUnlockDay = 9;

    // For parenting/unparenting the customer
    private Transform originalParent;
    private Table currentTable;

    // For Queue spots
    private ITarget currentQueueSpot;
    private Coroutine queueMove;
    // --- Seat fairness (FIFO) ---
    // Whoever enters SitAndDrink first should get first priority for seats.
    private static readonly LinkedList<CustomerBrain> SeatWaitList = new();
    private static readonly Dictionary<CustomerBrain, LinkedListNode<CustomerBrain>> SeatNodes = new();


    // For CustomerSpawner 
    private System.Action<GameObject> releaseToPool;
    public void Init(System.Action<GameObject> releasePool) { releaseToPool = releasePool; }
    public void DeSpawn() { releaseToPool?.Invoke(gameObject); }

    [Header("Coin and Tip Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float minTip = 0f;
    [SerializeField] private float maxTip = 20f;
    [Tooltip("Animation curve tipping: Most tip low, but rarely, very happy customers tip really high:")]
    [SerializeField] private AnimationCurve tipByMood = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Explicit Life Cycle
    private Coroutine runRoutine;

    public event Action<int> ReputationImpact;

    private void Awake()
    {
        originalParent = transform.parent;
        mover = (IMover)moverProvider;

        entry = GameObject.FindGameObjectWithTag("Entrance").gameObject.GetComponent<TransformTarget>();
        counter = GameObject.FindGameObjectWithTag("Counter").gameObject.GetComponent<TransformTarget>();
        exit = GameObject.FindGameObjectWithTag("Exit").gameObject.GetComponent<TransformTarget>();

        if (queue == null)
            queue = FindFirstObjectByType<QueueManager>();

        if (decor == null)
            decor = FindFirstObjectByType<DecorationManager>();

        // Resolve seating if not set
        if (seating == null)
            seating = SeatingManager.Instance != null
                ? SeatingManager.Instance
                : FindFirstObjectByType<SeatingManager>();

        // Safety log
        if (seating == null)
            Debug.LogError("CustomerBrain: SeatingManager not found in scene.");

        myMood = gameObject.GetComponent<CustomerMood>();

        if (orderBubble == null)
            orderBubble = gameObject.GetComponentInChildren<OrderBubble>();

        orderBubble.gameObject.SetActive(false);

        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();
    }

    public void Activate()
    {
        ResetObject();

        if (runRoutine != null)
            StopCoroutine(runRoutine);

        runRoutine = StartCoroutine(Run());
    }

    public void Deactivate()
    {
        if (runRoutine != null)
        {
            StopCoroutine(runRoutine);
            runRoutine = null;
        }
    }

    //private void OnEnable()
    //{
    //    StartCoroutine(DelayedStart());
    //}

    private IEnumerator DelayedStart()
    {
        yield return null; // wait 1 frame!!!
        ResetObject();
        StartCoroutine(Run());
    }

    private void SetState(CustomerState s)
    {
        current = s;
        stateRightNow = current;
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(s);
        }
    }

    IEnumerator Run()
    {
        yield return EnterCafe();
        yield return LookAround();
        yield return WaitInLine();
        yield return PlaceOrder();
        yield return SitAndDrink();
        yield return LeaveCafe();
    }

    IEnumerator EnterCafe()
    {
        SetState(CustomerState.EnteringCafe);

        // Decides whether to look around or go straight to counter (ordering)
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
            interestedInWaiting = false;
        else
            interestedInWaiting = true;

        yield return Go(entry);
    }

    // TO:DO
    // Customer should naturally wander around the cafe,
    // then only when it spots a decoration should they stop and look at it for a while.
    // This requires decorations to have some kind of "visibility" check from the customer's position.
    // For now, just randomly pick a decoration and walk to it.
    IEnumerator LookAround()
    {
        if (interestedInWaiting == false && decor != null)
        {
            SetState(CustomerState.Browsing);

            List<TransformTarget> tt = decor.GetListOfDecorations;
            decorationViewSpot = tt[UnityEngine.Random.Range(0, tt.Count)];

            ITarget randomViewTarget = SampleAroundTransformTargetRandomly(decorationViewSpot.Position, decorViewRadius);

            yield return Go(randomViewTarget);
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        }
    }

    IEnumerator WaitInLine()
    {
        // 1) Try seat immediately 
        TransformTarget seatTarget;
        if (seating.TryReserveRandomFreeSeat(out seatTarget))
        {
            mySeat = seatTarget;
            yield break; // seat acquired, continue pipeline
        }

        // 2) No seats -> join line (customers 5,6,...)
        SetState(CustomerState.WaitingInLine);
        queue.JoinLine(this);

        // Wait until I'm front and a seat becomes available
        while (true)
        {
            if (queue.IsFrontOfLine(this))
            {
                if (seating.TryReserveRandomFreeSeat(out seatTarget))
                {
                    // I have priority because I'm front
                    mySeat = seatTarget;

                    queue.LeaveLine(this); // shift everyone forward
                    break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        // stop queue movement coroutine if one is active
        if (queueMove != null)
        {
            StopCoroutine(queueMove);
            queueMove = null;
        }
    }

    IEnumerator PlaceOrder()
    {
        SetState(CustomerState.PlacingOrder);

        // ... order logic ...
        // Ordering time for now to simulate ordering
        //desiredDrink = OrderForARandomTea();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));

        orderBubble.gameObject.SetActive(true);
        orderBubble.VisualizeOrder(desiredDrink);

        // free the counter for the next customer
        queue.LeaveLine(this);
    }

    IEnumerator SitAndDrink()
    {
        if (mySeat == null)
        {
            Debug.LogError("CustomerBrain: mySeat is null when trying to sit.");
            yield break;
        }

        SetState(CustomerState.Sitting);

        var table = SeatingManager.Instance.GetTableForSeat(mySeat);
        if (table == null)
        {
            SeatingManager.Instance.ReleaseSeat(mySeat);
            mySeat = null;
            yield break;
        }

        yield return Go(mySeat);
        AttachToTable(table);

        // Wait until the correct drink shows up on THIS table
        while (currentTable == null || !currentTable.HasDrinkOfType(desiredDrink))
        {
            myMood.DecayPerSecond(moodDecayAmount, moodDecayRate);

            if (myMood.IsFedUp)
            {
                myMood.GetEmote(myMood.currentMoodValue);
                SetState(CustomerState.LeavingCafe);

                DailyCafeStats.Instance?.OnCustomerLost(myMood.currentMoodValue);
                // play here: angry animation or something

                yield return LeaveCafe();
                yield break;
            }

            yield return null;

        }

        // Order delivered, order bubble no longer needed
        orderBubble.gameObject.SetActive(false);
        emote.sprite = myMood.GetEmote(myMood.currentMoodValue);
        SetState(CustomerState.Drinking);
        yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 8f));
        emote.sprite = null;
        emote.gameObject.SetActive(false);

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

        CustomerServed?.Invoke(this);
        DailyCafeStats.Instance?.OnCustomerServed(myMood.currentMoodValue);
    }

    private void AttachToTable(Table table)
    {
        currentTable = table;


        // Keep world position/rotation as-is when parenting (important!)
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
        orderBubble.gameObject.SetActive(false);
    }

    IEnumerator LeaveCafe()
    {
        DequeueForSeat();

        SetState(CustomerState.LeavingCafe);

        orderBubble.gameObject.SetActive(false);

        DetachToTable();

        if (mySeat != null)
        {
            SeatingManager.Instance.ReleaseSeat(mySeat);
            mySeat = null;
        }

        ITarget randomExitTarget = SampleAroundTransformTargetRandomly(exit.Position, exitRadius);

        yield return Go(randomExitTarget);

        int repDelta = myMood.IsFedUp ? -3 : +1;
        ReputationImpact?.Invoke(repDelta);

        DeSpawn();
    }

    private ITarget SampleAroundTransformTargetRandomly(Vector3 basePos, float sampleRadius)
    {
        Vector2 offSet2D = UnityEngine.Random.insideUnitCircle * sampleRadius;
        Vector3 randomPos = new Vector3(
            basePos.x + offSet2D.x,
            basePos.y,
            basePos.z + offSet2D.y
        );

        ITarget randomTarget = new PointTarget(randomPos);
        return randomTarget;
    }

    private class PointTarget : ITarget
    {
        public Vector3 Position { get; }

        public PointTarget(Vector3 position)
        {
            Position = position;
        }
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

    private void DequeueForSeat()
    {
        if (!SeatNodes.TryGetValue(this, out var node)) return;
        SeatWaitList.Remove(node);
        SeatNodes.Remove(this);
    }

    private DrinkType OrderForARandomTea()
    {
        DrinkType randomDrinkType = (DrinkType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(DrinkType)).Length);
        return randomDrinkType;
    }

    private void GenerateOrder()
    {
        if (timeManager == null)
        {
            // Fallback: try to grab it if something went wrong
            timeManager = FindFirstObjectByType<TimeManager>();
            if (timeManager == null)
            {
                Debug.LogWarning("CustomerBrain: TimeManager not found, defaulting to tea only.");
                desiredDrink = OrderForARandomTea();
                return;
            }
        }

        // Always order a tea
        desiredDrink = OrderForARandomTea();

        // Prepare main dish container for this customer
        desiredDish = new MainDish();
        desiredDish.breakfast = default;
        desiredDish.lunch = default;
        desiredDish.dinner = default;

        int currentDay = timeManager.Day;
        MealTime currentMealTime = timeManager.GetMealTime();

        switch (currentMealTime)
        {
            case MealTime.BreakfastTime:
                if (currentDay >= breakfastUnlockDay)
                {
                    desiredDish.breakfast = desiredDish.OrderRandomBreakfastItem();
                    Debug.Log($"[Order] Day {currentDay}, Breakfast unlocked. Dish = {desiredDish.breakfast}");
                }
                break;

            case MealTime.LunchTime:
                if (currentDay >= lunchUnlockDay)
                {
                    desiredDish.lunch = desiredDish.OrderRandomLunchItem();
                    Debug.Log($"[Order] Day {currentDay}, Lunch unlocked. Dish = {desiredDish.lunch}");
                }
                break;

            case MealTime.DinnerTime:
                if (currentDay >= dinnerUnlockDay)
                {
                    desiredDish.dinner = desiredDish.OrderRandomDinnerItem();
                    Debug.Log($"[Order] Day {currentDay}, Dinner unlocked. Dish = {desiredDish.dinner}");
                }
                break;
        }
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

    public void ResetObject()
    {
        SetState(CustomerState.EnteringCafe);
        myMood.ResetMood();

        // Generate a fresh order for this spawn
        GenerateOrder();

        // Make sure order bubble starts hidden
        if (orderBubble != null)
            orderBubble.gameObject.SetActive(false);
    }
}

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
        yield return new WaitForEndOfFrame();
    }

    IEnumerator PlaceOrder()
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator SitAndDrink()
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator LeaveCafe()
    {
        SetState(CustomerState.LeavingCafe);
        yield return Go(exit);
    }


    IEnumerator Go(ITarget t)
    {
        bool done = false;
        void Handler() { done = true; }
        mover.ReachedTarget += Handler;
        mover.GoTo(t);
        yield return new WaitUntil(() => done);
        mover.ReachedTarget -= Handler;
    }
}

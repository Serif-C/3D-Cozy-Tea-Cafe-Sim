using UnityEngine;
using System;

public enum CustomerStates
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

    public CustomerStates current { get; private set; }
    public event Action<CustomerStates> currentChanged;

    private void Awake()
    {
        mover = (IMover)moverProvider;
    }

    private void Start()
    {
        //StartCoroutine(Run());
    }

}

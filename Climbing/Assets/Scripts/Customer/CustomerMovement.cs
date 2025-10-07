using System;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour, IMover
{
    [SerializeField] private float moveSpeed = 2f;
    private NavMeshAgent agent;

    public bool IsMoving    // same as `IsMoving =>agent.hasPath && agent.remainingDistance > agent.stoppingDistance;`
    {
        get
        {
            return agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        }
    }

    public event Action ReachedTarget;

    public void GoTo(ITarget target)
    {
        agent.isStopped = false;
        agent.SetDestination(target.Position);
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    public void Warp(Vector3 position)
    {
        agent.Warp(position);
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        
    }

}

using System;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour, IMover
{
    [SerializeField] private float moveSpeed = 2f;
    private NavMeshAgent agent;
    private ITarget current;

    public bool IsMoving    // same as `IsMoving =>agent.hasPath && agent.remainingDistance > agent.stoppingDistance;`
    {
        get
        {
            return agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        }
    }

    public event Action ReachedTarget;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    public void GoTo(ITarget target)
    {
        current = target;
        agent.isStopped = false;
        agent.SetDestination(target.Position);
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
        current = null;
    }

    public void Warp(Vector3 position)
    {
        agent.Warp(position);
        current = null;
    }


    private void Update()
    {
        // Basic arrival detection; tweak thresholds if needed
        if (current != null && !IsMoving)
        {
            var done = ReachedTarget; // copy for thread-safety pattern
            current = null;
            if (done != null)
            {
                done();
            }
        }
    }

}

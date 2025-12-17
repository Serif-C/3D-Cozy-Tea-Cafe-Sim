using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Anchor")]
    [SerializeField] private TransformTarget counter;

    [Tooltip("Where slot 0 stands relative to the counter (in meters).")]
    [SerializeField] private float slotOffsetFromCounter = 1.2f;

    [Tooltip("Distance between customers in the line (in meters).")]
    [SerializeField] private float slotSpacing = 0.9f;

    [Tooltip("Direction the line extends. If emtpy, uses -counter.forward.")]
    [SerializeField] private Transform lineDirectionSource;

    [Tooltip("Navmesh sampling max distance (helps keep targets reachable.")]
    [SerializeField] private float navMeshSampleDistance = 2.0f;
    [SerializeField] private float minSeparationFactor = 0.85f;      // 0.85 * slotSpacing minimum gap
    [SerializeField] private int maxPushAttempts = 6;

    private readonly List<Vector3> _cachedSlots = new();

    private CustomerBrain atCounter;
    public ITarget CounterTarget => counter;

    // 0 = front
    private readonly List<CustomerBrain> line = new();

    private void Awake()
    {
        if (counter == null)
            counter = GameObject.FindGameObjectWithTag("Counter").GetComponent<TransformTarget>();

        if (lineDirectionSource == null && counter != null)
            lineDirectionSource = counter.transform;
    }



    // Customer joins the queue; return the target they should go to now
    public ITarget Join(CustomerBrain customer)
    {
        if (!line.Contains(customer))
            line.Add(customer);

        ReAssignAll();  // everyone gets an updated slot target

        // return this customer's current slot target
        int idx = line.IndexOf(customer);
        return GetTargetForIndex(idx);
                
    }

    public void Leave(CustomerBrain customer)
    {
        int i = line.IndexOf(customer);
        if (i < 0) return;

        line.RemoveAt(i);
        ReAssignAll();
    }

    public bool IsMyTurn(CustomerBrain customer)
    {
        return line.Count > 0 && line[0] == customer;
    }

    public bool TryAcquireCounter(CustomerBrain customer)
    {
        if (IsMyTurn(customer) && (atCounter == null || atCounter == customer))
        {
            atCounter = customer;
            Leave(customer);    // peel off front; re-assign all
            return true;
        }

        return false;
    }

    public void ReleaseCounter(CustomerBrain customer)
    {
        if (atCounter == customer)
            atCounter = null;
    }

    public void ReAssignAll()
    {
        BuildSlotCache(line.Count);

        for (int i = 0; i < line.Count; i++)
        {
            var c = line[i];
            if (c == null) continue;

            c.UpdateQueueTarget(new PointTarget(_cachedSlots[i]));
        }
    }

    private void BuildSlotCache(int count)
    {
        _cachedSlots.Clear();
        if (count <= 0) return;
        if (counter == null) return;

        // Direction “away from counter”
        Transform src = lineDirectionSource != null ? lineDirectionSource : counter.transform;
        Vector3 backDir = -src.forward;
        backDir.y = 0f;
        backDir = backDir.sqrMagnitude > 0.0001f ? backDir.normalized : Vector3.back;

        // Slot 0 desired
        Vector3 desired0 = counter.transform.position + backDir * slotOffsetFromCounter;
        Vector3 p0 = SampleToNavMesh(desired0);
        _cachedSlots.Add(p0);

        // Slots 1..n: build from previous to enforce spacing
        float minGap = slotSpacing * minSeparationFactor;

        for (int i = 1; i < count; i++)
        {
            Vector3 prev = _cachedSlots[i - 1];
            Vector3 desired = prev + backDir * slotSpacing;

            Vector3 p = SampleToNavMesh(desired);

            // If sampling collapses too close to prev, push farther until separated
            int attempts = 0;
            while (Vector3.Distance(p, prev) < minGap && attempts < maxPushAttempts)
            {
                desired += backDir * (slotSpacing * 0.5f);
                p = SampleToNavMesh(desired);
                attempts++;
            }

            // Last fallback: if still too close, just use the desired (keeps uniqueness)
            if (Vector3.Distance(p, prev) < minGap)
                p = desired;

            _cachedSlots.Add(p);
        }
    }

    private ITarget GetTargetForIndex(int index)
    {
        Vector3 pos = GetWorldPosForIndex(index);
        pos = SampleToNavMesh(pos);
        return new PointTarget(pos);
    }

    private Vector3 GetWorldPosForIndex(int index)
    {
        if (counter == null) return Vector3.zero;

        // Base position for the head of the line
        Vector3 basePos = counter.transform.position;

        // Choose a direction “away from counter” to extend the line
        Transform src = lineDirectionSource != null ? lineDirectionSource : counter.transform;
        Vector3 backDir = -src.forward; // line goes backward from counter
        backDir.y = 0f;
        backDir = backDir.sqrMagnitude > 0.0001f ? backDir.normalized : Vector3.back;

        // Slot 0 is offset from counter, then each index adds spacing
        float dist = slotOffsetFromCounter + (index * slotSpacing);

        return basePos + backDir * dist;
    }

    private Vector3 SampleToNavMesh(Vector3 p)
    {
        if (NavMesh.SamplePosition(p, out var hit, navMeshSampleDistance, NavMesh.AllAreas))
            return hit.position;

        return p;
    }

    private class PointTarget : ITarget
    {
        public Vector3 Position { get; }
        public PointTarget(Vector3 position) => Position = position;
    }

    private void OnDrawGizmosSelected()
    {
        if (counter == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = GetWorldPosForIndex(i);
            Gizmos.DrawSphere(p + Vector3.up * 0.05f, 0.08f);
        }
    }
}

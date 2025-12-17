using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Anchor")]
    [SerializeField] private TransformTarget counter;

    [Header("Line Layout")]
    [SerializeField] private float slotOffsetFromCounter = 1.2f;
    [SerializeField] private float slotSpacing = 0.9f;
    [SerializeField] private Transform lineDirectionSource; // if null uses counter

    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleDistance = 0.35f;

    // 0 = front
    private readonly List<CustomerBrain> line = new();

    private void Awake()
    {
        if (counter == null)
            counter = GameObject.FindGameObjectWithTag("Counter").GetComponent<TransformTarget>();

        if (lineDirectionSource == null && counter != null)
            lineDirectionSource = counter.transform;
    }

    public bool IsInLine(CustomerBrain c) => c != null && line.Contains(c);

    public void JoinLine(CustomerBrain c)
    {
        if (c == null) return;
        if (!line.Contains(c))
            line.Add(c);

        RefreshLinePositions();
    }

    public void LeaveLine(CustomerBrain c)
    {
        if (c == null) return;
        int i = line.IndexOf(c);
        if (i < 0) return;

        line.RemoveAt(i);
        RefreshLinePositions();
    }

    public bool IsFrontOfLine(CustomerBrain c)
    {
        return line.Count > 0 && line[0] == c;
    }

    public void RefreshLinePositions()
    {
        for (int i = 0; i < line.Count; i++)
        {
            var c = line[i];
            if (c == null) continue;

            Vector3 p = GetSlotWorldPos(i);
            p = SampleToNavMesh(p);

            c.UpdateQueueTarget(new PointTarget(p));
        }
    }

    private Vector3 GetSlotWorldPos(int index)
    {
        if (counter == null) return Vector3.zero;

        Transform src = lineDirectionSource != null ? lineDirectionSource : counter.transform;

        Vector3 backDir = -src.forward;
        backDir.y = 0f;
        backDir = backDir.sqrMagnitude > 0.0001f ? backDir.normalized : Vector3.back;

        float dist = slotOffsetFromCounter + (index * slotSpacing);
        return counter.transform.position + backDir * dist;
    }

    private Vector3 SampleToNavMesh(Vector3 desired)
    {
        // Keep this tight: large radii cause multiple slots to collapse to the same nearest point.
        if (NavMesh.SamplePosition(desired, out var hit, navMeshSampleDistance, NavMesh.AllAreas))
            return hit.position;

        return desired;
    }

    private class PointTarget : ITarget
    {
        public Vector3 Position { get; }
        public PointTarget(Vector3 p) => Position = p;
    }

    private void OnDrawGizmosSelected()
    {
        if (counter == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 8; i++)
        {
            Gizmos.DrawSphere(GetSlotWorldPos(i) + Vector3.up * 0.05f, 0.08f);
        }
    }
}

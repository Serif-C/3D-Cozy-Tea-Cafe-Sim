using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Search;
using UnityEngine;

public class SeatingManager : MonoBehaviour
{
    public static SeatingManager Instance { get; private set; }
    //[SerializeField] private TransformTarget[] seats;
    [SerializeField] private List<TransformTarget> seats = new List<TransformTarget>();
    private readonly HashSet<TransformTarget> reserved = new HashSet<TransformTarget>();
    private readonly Dictionary<TransformTarget, Table> seatToTable = new Dictionary<TransformTarget, Table>();

    [Header("Chair -> Table validation")]
    [SerializeField] private LayerMask tableMask;               // layer tablecolliders are on
    [SerializeField] private float tableConeMaxDistance = 1.75f;
    [SerializeField] private float tableConeHalfAngleDeg = 25f; // 25° each side (~50° FOV)
    [SerializeField] private float tableCheckHeight = 0.6f;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var tag = GameObject.FindGameObjectsWithTag("Seats");
        var list = new List<TransformTarget>(tag.Length);

        foreach (var s in tag)
        {
            if (s.TryGetComponent(out TransformTarget seat))
            {
                list.Add(seat);
            }
        }

        seats = list;

        Debug.Log($"[Seating] Preplaced seats found: {seats.Count}");
    }

    public bool TryReserveRandomFreeSeat(out TransformTarget seat)
    {
        seat = null;
        if (seats.Count == 0) return false;

        List<TransformTarget> free = new List<TransformTarget>(seats.Count);

        for (int i = 0; i < seats.Count; i++)
        {
            var s = seats[i];
            if (s == null) continue;

            // Resolve or refresh chair -> table association
            if (!seatToTable.TryGetValue(s, out var table) || table == null)
            {
                TryFindTableInFront(s, out table);
                seatToTable[s] = table;
            }

            // RULE: must have a table in front
            if (table == null) continue;

            // Already reserved by another customer?
            if (reserved.Contains(s)) continue;

            // If Table tracks a single-occupancy flag, keep honoring it
            if (table.IsTableOccupied()) continue;

            free.Add(s);
        }

        if (free.Count == 0) return false;

        var pick = free[UnityEngine.Random.Range(0, free.Count)];

        // Reserve
        reserved.Add(pick);

        // Mirror table occupancy so your existing code paths still work
        if (seatToTable.TryGetValue(pick, out var pickedTable) && pickedTable != null)
            pickedTable.SetOccupiedValue(true);

        seat = pick;
        return true;
    }

    public void ReleaseSeat(TransformTarget seat)
    {
        if (seat == null) return;

        if (reserved.Remove(seat))
        {
            if (seatToTable.TryGetValue(seat, out var table) && table != null)
            {
                table.SetOccupiedValue(false);
            }
        }
    }

    public void AddSeat(Transform seatTransform)
    {
        if (seatTransform == null) return;
        var tt = seatTransform.GetComponent<TransformTarget>();
        if (tt == null) tt = seatTransform.gameObject.AddComponent<TransformTarget>();

        if (!seats.Contains(tt)) seats.Add(tt);

        // Pre-cache the associated table if we can find one
        TryFindTableInFront(tt, out var table);
        seatToTable[tt] = table; // may be null if no table in front yet
    }

    public void RemoveSeat(Transform seatTransform)
    {
        if (seatTransform == null) return;
        var tt = seatTransform.GetComponent<TransformTarget>();
        if (tt == null) return;

        reserved.Remove(tt);
        seats.Remove(tt);
        seatToTable.Remove(tt);
    }

    public Table GetTableForSeat(TransformTarget seat)
    {
        if (seat == null) return null;

        if (!seatToTable.TryGetValue(seat, out var table) || table == null)
        {
            TryFindTableInFront(seat, out table);
            seatToTable[seat] = table; // cache for next time
        }
        return table;
    }

    private bool TryFindTableInFront(TransformTarget seat, out Table table)
    {
        table = null;
        if (seat == null) return false;

        Vector3 seatPos = seat.Position + Vector3.up * tableCheckHeight;
        Vector3 seatFwd = seat.transform.forward.normalized;

        // 1) Collect nearby colliders (candidate tables)
        Collider[] hits = Physics.OverlapSphere(
            seatPos,
            tableConeMaxDistance,
            tableMask,
            QueryTriggerInteraction.Ignore
        );
        if (hits == null || hits.Length == 0) return false;

        Table bestTable = null;
        Vector3 bestPoint = default;
        float bestSqr = float.PositiveInfinity;

        // 2) Filter by cone (angle) and choose nearest collider point
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            var t = col.GetComponentInParent<Table>();
            if (t == null) continue;

            // closest *surface* point on the table to the chair
            Vector3 p = col.ClosestPoint(seatPos);
            Vector3 toP = (p - seatPos);
            float dist = toP.magnitude;
            if (dist <= 1e-4f) continue; // effectively same point

            Vector3 dir = toP / dist;

            // ANGLE test -> inside cone?
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(seatFwd, dir), -1f, 1f)) * Mathf.Rad2Deg;
            if (angle > tableConeHalfAngleDeg) continue; // outside cone

            // Keep nearest inside the cone
            float d2 = dist * dist;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                bestPoint = p;
                bestTable = t;
            }
        }

        if (bestTable == null) return false;

        table = bestTable;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (seats == null) return;

        for (int i = 0; i < seats.Count; i++)
        {
            var s = seats[i];
            if (s == null) continue;

            Vector3 pos = s.Position + Vector3.up * tableCheckHeight;
            Vector3 fwd = s.transform.forward;

            // draw cone edges
            float ang = tableConeHalfAngleDeg;
            Quaternion qL = Quaternion.AngleAxis(-ang, Vector3.up);
            Quaternion qR = Quaternion.AngleAxis(ang, Vector3.up);

            Vector3 leftDir = qL * fwd;
            Vector3 rightDir = qR * fwd;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + leftDir * tableConeMaxDistance);
            Gizmos.DrawLine(pos, pos + rightDir * tableConeMaxDistance);

            // base arc (rough)
            Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
            const int seg = 16;
            Vector3 prev = pos + leftDir * tableConeMaxDistance;
            for (int sIdx = 1; sIdx <= seg; sIdx++)
            {
                float t = (float)sIdx / seg;
                float a = Mathf.Lerp(-ang, ang, t);
                Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * fwd;
                Vector3 curr = pos + dir * tableConeMaxDistance;
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }
    }

}

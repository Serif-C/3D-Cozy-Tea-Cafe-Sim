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
    [SerializeField] private float tableCheckDistance = 1.25f;  // how far in front to look for a chair
    [SerializeField] private float tableCheckHeight = 0.6f;
    [SerializeField] private float tableCheckRadius = 0.15f;

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
        //var listOfTables = new List<Table>();

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

    //public bool TryReserveRandomFreeSeat(out TransformTarget seat)
    //{
    //    seat = null;

    //    // Build a list of free seats
    //    var free = new List<int>(seats.Count);

    //    for (int i = 0; i < seats.Count; i++)
    //    {
    //        var table = seats[i].GetComponentInParent<Table>();
    //        if (table != null && !table.IsTableOccupied())
    //        { 
    //            free.Add(i); 
    //        }
    //    }

    //    if (free.Count == 0)
    //        return false;   // if nothing is free, let caller wait

    //    // Pick one at random
    //    int pick = free[Random.Range(0, free.Count)];
    //    var pickedSeat = seats[pick];

    //    // Reserve immidiately to prevent customers from racing to it
    //    pickedSeat.GetComponentInParent<Table>().SetOccupiedValue(true);

    //    seat = pickedSeat;
    //    return true;

    //}

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

    //private bool TryFindTableInFront(TransformTarget seat, out Table table)
    //{
    //    table = null;
    //    if (seat == null) return false;

    //    Vector3 start = seat.transform.position + Vector3.up * tableCheckHeight;
    //    Vector3 dir = seat.transform.forward;  // front = +Z of chair
    //    float dist = tableCheckDistance;

    //    // SphereCast is more forgiving than a Raycast
    //    if (Physics.SphereCast(start, tableCheckRadius, dir, out var hit, dist, tableMask, QueryTriggerInteraction.Ignore))
    //    {
    //        table = hit.collider.GetComponentInParent<Table>();
    //        return table != null;
    //    }

    //    return false;
    //}

    private bool TryFindTableInFront(TransformTarget seat, out Table table)
    {
        table = null;
        if (seat == null) return false;

        // 1) Find the nearest Table in range (independent of seat forward)
        Vector3 pos = seat.transform.position + Vector3.up * tableCheckHeight;
        Collider[] hits = Physics.OverlapSphere(pos, tableCheckDistance, tableMask, QueryTriggerInteraction.Ignore);

        Table nearest = null;
        float best = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Table t = hits[i].GetComponentInParent<Table>();
            if (t == null) continue;

            float d = Vector3.Distance(seat.transform.position, t.transform.position);
            if (d < best)
            {
                best = d;
                nearest = t;
            }
        }

        if (nearest == null) return false;

        // 2) Facing rule: seat must point (roughly) toward the table
        Vector3 toTable = (nearest.transform.position - seat.transform.position).normalized;
        float facing = Vector3.Dot(seat.transform.forward, toTable); // +1 = directly facing

        // tweak the threshold as you like (0.35 ~ within ~70° cone)
        if (facing < 0.35f) return false;

        table = nearest;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (seats == null) return;

        for (int i = 0; i < seats.Count; i++)
        {
            var s = seats[i];
            if (s == null) continue;

            Vector3 pos = s.transform.position + Vector3.up * tableCheckHeight;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, tableCheckDistance);

            // facing ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + s.transform.forward * tableCheckDistance);
        }
    }
}

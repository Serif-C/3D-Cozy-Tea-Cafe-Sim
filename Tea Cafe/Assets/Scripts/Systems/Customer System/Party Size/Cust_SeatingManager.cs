using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Cust_SeatingManager : MonoBehaviour
{

    /// <summary>
    /// Manages chair (seat) reservations and resolves each chair to the Table it faces.
    /// - Supports reserving multiple seats at a single table (party sizes 1..4).
    /// - Table "occupied" is now derived from whether ANY seats at that table are reserved.
    /// </summary>
    /// 

    public static Cust_SeatingManager Instance {  get; private set; }

    [SerializeField] private List<TransformTarget> seats =  new();

    // Reserved seats
    private readonly HashSet<TransformTarget> reservedSeats = new();

    // Cached mappings
    private readonly Dictionary<TransformTarget, Table> seatToTable = new();
    private readonly Dictionary<Table, List<TransformTarget>> tableToSeats = new();

    [Header("Chair -> Table detection")]
    [SerializeField] private LayerMask tableMask;
    [SerializeField] private float tableConeMaxDistance = 1.75f;
    [SerializeField] private float tableConeHalfAngleDeg = 25f; // each side
    [SerializeField] private float tableCheckHeight = 0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Auto-discover all seats by tag
        var tagged = GameObject.FindGameObjectsWithTag("Seats");
        var list = new List<TransformTarget>(tagged.Length);
        foreach (var go in tagged)
        {
            if (go != null && go.TryGetComponent(out TransformTarget seat))
                list.Add(seat);
        }
        seats = list;

        RebuildSeatTableCache();
        Debug.Log($"[Seating] Seats found: {seats.Count}");
    }

    /// <summary>Back-compat: reserve exactly one seat.</summary>
    public bool TryReserveRandomFreeSeat(out TransformTarget seat)
    {
        seat = null;
        if (!TryReserveSeatsForParty(1, out var partySeats, out _))
            return false;

        seat = partySeats[0];
        return true;
    }

    // <summary>
    /// Reserve N seats (1..4) at the same table.
    /// </summary>
    public bool TryReserveSeatsForParty(int partySize, out List<TransformTarget> reservedOut, out Table tableOut)
    {
        reservedOut = null;
        tableOut = null;

        partySize = Mathf.Clamp(partySize, 1, 4);
        if (seats.Count == 0) return false;

        RebuildSeatTableCacheIfNeeded();

        // Candidate tables that have enough free seats
        var candidates = new List<Table>();
        foreach (var kvp in tableToSeats)
        {
            var table = kvp.Key;
            var seatsForTable = kvp.Value;

            if (table == null || seatsForTable == null || seatsForTable.Count == 0) continue;

            int freeCount = 0;
            for (int i = 0; i < seatsForTable.Count; i++)
            {
                var s = seatsForTable[i];
                if (s == null) continue;
                if (reservedSeats.Contains(s)) continue;
                freeCount++;
            }

            if (freeCount >= partySize)
                candidates.Add(table);
        }

        if (candidates.Count == 0) return false;

        var chosenTable = candidates[Random.Range(0, candidates.Count)];
        var chosenSeatsList = tableToSeats[chosenTable];

        var pickedSeats = new List<TransformTarget>(partySize);
        for (int i = 0; i < chosenSeatsList.Count && pickedSeats.Count < partySize; i++)
        {
            var s = chosenSeatsList[i];
            if (s == null) continue;
            if (reservedSeats.Contains(s)) continue;
            pickedSeats.Add(s);
        }

        if (pickedSeats.Count < partySize) return false;

        // Reserve them
        for (int i = 0; i < pickedSeats.Count; i++)
            reservedSeats.Add(pickedSeats[i]);

        // Mirror occupancy for legacy logic
        chosenTable.SetOccupiedValue(true);

        reservedOut = pickedSeats;
        tableOut = chosenTable;
        return true;
    }

    public void ReleaseSeat(TransformTarget seat)
    {
        if (seat == null) return;
        if (!reservedSeats.Remove(seat)) return;

        if (seatToTable.TryGetValue(seat, out var table) && table != null)
        {
            bool anyReservedAtTable = false;
            if (tableToSeats.TryGetValue(table, out var seatsForTable) && seatsForTable != null)
            {
                for (int i = 0; i < seatsForTable.Count; i++)
                {
                    var s = seatsForTable[i];
                    if (s == null) continue;
                    if (reservedSeats.Contains(s))
                    {
                        anyReservedAtTable = true;
                        break;
                    }
                }
            }

            table.SetOccupiedValue(anyReservedAtTable);
        }
    }
    public Table GetTableForSeat(TransformTarget seat)
    {
        if (seat == null) return null;
        if (seatToTable.TryGetValue(seat, out var table))
            return table;
        return null;
    }

    // Optional hooks if you dynamically build seats
    public void AddSeat(Transform seatTransform)
    {
        if (seatTransform == null) return;

        var tt = seatTransform.GetComponent<TransformTarget>();
        if (tt == null) tt = seatTransform.gameObject.AddComponent<TransformTarget>();
        if (!seats.Contains(tt)) seats.Add(tt);

        TryFindTableInFront(tt, out var table);
        seatToTable[tt] = table;

        if (table != null)
        {
            if (!tableToSeats.TryGetValue(table, out var list))
            {
                list = new List<TransformTarget>();
                tableToSeats[table] = list;
            }
            if (!list.Contains(tt)) list.Add(tt);
        }
    }

    public void RemoveSeat(Transform seatTransform)
    {
        if (seatTransform == null) return;
        var tt = seatTransform.GetComponent<TransformTarget>();
        if (tt == null) return;

        reservedSeats.Remove(tt);
        seats.Remove(tt);

        if (seatToTable.TryGetValue(tt, out var table) && table != null)
        {
            if (tableToSeats.TryGetValue(table, out var list) && list != null)
                list.Remove(tt);
        }

        seatToTable.Remove(tt);
    }

    private void RebuildSeatTableCacheIfNeeded()
    {
        // cheap check: if sizes mismatch, rebuild
        if (seatToTable.Count != seats.Count)
            RebuildSeatTableCache();
    }

    private void RebuildSeatTableCache()
    {
        seatToTable.Clear();
        tableToSeats.Clear();

        for (int i = 0; i < seats.Count; i++)
        {
            var seat = seats[i];
            if (seat == null) continue;

            if (TryFindTableInFront(seat, out var table))
            {
                seatToTable[seat] = table;

                if (!tableToSeats.TryGetValue(table, out var list))
                {
                    list = new List<TransformTarget>();
                    tableToSeats[table] = list;
                }
                if (!list.Contains(seat)) list.Add(seat);
            }
            else
            {
                seatToTable[seat] = null;
            }
        }
    }

    private bool TryFindTableInFront(TransformTarget seat, out Table table)
    {
        table = null;
        if (seat == null) return false;

        Vector3 origin = seat.Position + Vector3.up * tableCheckHeight;
        Vector3 forward = seat.transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            0.2f,
            forward,
            tableConeMaxDistance,
            tableMask,
            QueryTriggerInteraction.Ignore
        );

        Table best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var go = hit.collider.gameObject;

            if (!go.TryGetComponent(out Table t))
                t = go.GetComponentInParent<Table>();

            if (t == null) continue;

            Vector3 toTable = (t.transform.position - seat.transform.position);
            toTable.y = 0f;
            float dist = toTable.magnitude;
            if (dist < 0.01f) continue;

            float angle = Vector3.Angle(forward, toTable.normalized);
            if (angle > tableConeHalfAngleDeg) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        if (best != null)
        {
            table = best;
            return true;
        }

        return false;
    }
}

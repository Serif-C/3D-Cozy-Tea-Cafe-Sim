using System.Collections.Generic;
using UnityEngine;

public class CustomerParty : MonoBehaviour
{
    public CustomerBrain Leader { get; private set; }

    private readonly List<CustomerBrain> _members = new();
    private readonly List<TransformTarget> _reservedSeats = new();

    public IReadOnlyList<CustomerBrain> Members => _members;
    public Table ReservedTable { get; private set; }

    public int Size => _members.Count;

    public bool HasReservedSeats =>
        ReservedTable != null &&
        _reservedSeats.Count == _members.Count &&
        _members.Count > 0;

    public void AddMember(CustomerBrain brain, bool isLeader)
    {
        if (brain == null) return;
        if (!_members.Contains(brain))
            _members.Add(brain);

        if (isLeader || Leader == null)
            Leader = brain;
    }

    public void SetReservedSeating(Table table, List<TransformTarget> seats)
    {
        ReservedTable = table;
        _reservedSeats.Clear();
        if (seats != null) _reservedSeats.AddRange(seats);
    }

    public TransformTarget GetSeatFor(CustomerBrain member)
    {
        if (member == null) return null;
        int idx = _members.IndexOf(member);
        if (idx < 0 || idx >= _reservedSeats.Count) return null;
        return _reservedSeats[idx];
    }

    public List<TransformTarget> GetAllSeatsCopy()
    {
        return new List<TransformTarget>(_reservedSeats);
    }
}

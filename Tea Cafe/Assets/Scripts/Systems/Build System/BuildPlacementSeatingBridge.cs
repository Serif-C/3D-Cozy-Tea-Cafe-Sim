using UnityEngine;
using TeaShop.Systems.Building;

public class BuildPlacementSeatingBridge : MonoBehaviour
{
    [SerializeField] private PlacementRegistry registry;
    [SerializeField] private SeatingManager seating;

    private void Awake()
    {
        if (registry == null) registry = FindFirstObjectByType<PlacementRegistry>();
        if (seating == null) seating = FindFirstObjectByType<SeatingManager>();
    }

    private void OnEnable()
    {
        if (registry != null) registry.Placed += HandlePlaced;
        if (registry != null) registry.Removed += HandleRemoved;
    }

    private void OnDisable()
    {
        if (registry != null) registry.Placed -= HandlePlaced;
        if (registry != null) registry.Removed -= HandleRemoved;
    }

    private void HandlePlaced(PlaceableInstance inst)
    {
        if (inst == null) return;
        PlaceableItemConfig cfg = inst.GetConfig();
        if (cfg == null) return;

        if (cfg.Category == PlaceableCategory.Chair)
        {
            Transform seat = inst.transform;
            seating.AddSeat(seat);
        }
    }

    private void HandleRemoved(PlaceableInstance inst)
    {
        if (inst == null) return;
        PlaceableItemConfig cfg = inst.GetConfig();
        if (cfg == null) return;

        if (cfg.Category == PlaceableCategory.Chair)
        {
            Transform seat = inst.transform;
            seating.RemoveSeat(seat);
        }
    }
}

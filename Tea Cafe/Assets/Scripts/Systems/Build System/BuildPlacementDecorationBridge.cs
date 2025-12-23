using TeaShop.Systems.Building;
using UnityEngine;

public class BuildPlacementDecorationBridge : MonoBehaviour
{
    [SerializeField] private PlacementRegistry registry;

    private void Awake()
    {
        if (registry == null) registry = FindFirstObjectByType<PlacementRegistry>();
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
        if (cfg.Category == PlaceableCategory.Decoration)
        {
            Transform decoration = inst.transform;
            DecorationManager.Instance.AddDecoration(decoration);
        }
    }

    private void HandleRemoved(PlaceableInstance inst)
    {
        if (inst == null) return;
        PlaceableItemConfig cfg = inst.GetConfig();
        if (cfg == null) return;
        if (cfg.Category == PlaceableCategory.Decoration)
        {
            Transform decoration = inst.transform;
            DecorationManager.Instance.RemoveDecoration(decoration);
        }
    }
}

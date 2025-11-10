using UnityEngine;
using System.Collections.Generic;
using TeaShop.Systems.Building;

public class BuildSaveAdapter : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlacementRegistry registry;

    [Tooltip("All placeable configs placed in this scene. IDs must be unique.")]
    [SerializeField] private PlaceableItemConfig[] catalog;

    private Dictionary<string, PlaceableItemConfig> byId;
    private bool isQuitting;

    private void Awake()
    {
        if (registry == null) registry = FindFirstObjectByType<PlacementRegistry>();

        // Build id -> config map
        byId = new Dictionary<string, PlaceableItemConfig>(catalog != null ? catalog.Length: 0); 
        if (catalog != null )
        {
            foreach (var cfg in catalog)
            {
                if (cfg != null && !string.IsNullOrEmpty(cfg.Id))
                {
                    byId[cfg.Id] = cfg;
                }
            }
        }

        // LOAD
        var data = SaveSystem.Load();   // Maybe null on first run
        if (data != null && data.placements != null && data.placements.Length > 0)
        {
            registry.FromPlacementRecords(data.placements, ResolveConfig);
        }

        Debug.Log($"[Load] placements={(data?.placements?.Length ?? 0)}");

    }

    private void OnEnable()
    {
        if (registry == null) return;
        registry.Placed += OnPlaced;
        registry.Removed += OnRemoved;
    }

    private void OnDisable()
    {
        if (registry == null) return;
        registry.Placed -= OnPlaced;
        registry.Removed -= OnRemoved;
    }


    private PlaceableItemConfig ResolveConfig(string id)
    {
        return (id != null && byId != null && byId.TryGetValue(id, out var cfg)) ? cfg : null;
    }

    public void SaveNow()
    {
        // Don’t write if exiting play mode or not in play
        if (!Application.isPlaying || isQuitting) return;

        // Merge with existing save to preserve fields you already store (e.g., wallet)
        var data = SaveSystem.Load() ?? new PlayerSaveData();
        data.placements = registry.ToPlacementRecords();
        SaveSystem.Save(data);

        Debug.Log($"[Save] placements={registry.ToPlacementRecords().Length} {Application.persistentDataPath}");

    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnPlaced(PlaceableInstance _) { SaveNow(); }
    private void OnRemoved(PlaceableInstance _) { SaveNow(); }
}

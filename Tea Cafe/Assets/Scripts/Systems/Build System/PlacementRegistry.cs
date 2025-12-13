using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TeaShop.Systems.Building
{
    public class PlacementRegistry : MonoBehaviour
    {
        public event Action<PlaceableInstance> Placed;
        public event Action<PlaceableInstance> Removed;

        private readonly List<PlaceableInstance> instances = new List<PlaceableInstance>();

        public void Register(PlaceableInstance inst)
        {
            if (inst == null) return;
            if (!instances.Contains(inst)) instances.Add(inst);
            //Action<PlaceableInstance> h = Placed;
            //if (h != null) h(inst);
            Placed?.Invoke(inst);
        }

        public void UnRegister(PlaceableInstance inst)
        {
            if (inst == null) return;
            if (instances.Remove(inst))
            {
                //Action<PlaceableInstance> h = Removed;
                //if (h != null) h(inst);
                Removed?.Invoke(inst);
            }
        }

        public IReadOnlyList<PlaceableInstance> All() { return instances; }

        public bool OverlapsFootprint(PlaceableItemConfig cfg, Vector3 worldPosition, Quaternion worldRotation, PlaceableInstance ignore = null, PlacementGrid grid = null)
        {
            if (cfg == null) return false;
            if (grid == null) return false;

            var cells = PlacementFootprint.GetOccupiedCells(cfg, worldPosition, worldRotation, grid);

            for (int i = 0; i < instances.Count; i++)
            {
                var inst = instances[i];
                if (inst == null || inst == ignore) continue;

                var icfg = inst.GetConfig();
                if (icfg == null) continue;

                var otherCells = PlacementFootprint.GetOccupiedCells(icfg, inst.transform.position, inst.transform.rotation, grid);

                // any overlap?
                for (int a = 0; a < cells.Count; a++)
                {
                    for (int b = 0; b < otherCells.Count; b++)
                    {
                        if (cells[a] == otherCells[b])
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Convert currently registered instances to serializable records for saving.
        /// </summary>
        public PlacementRecord[] ToPlacementRecords()
        {
            // 'instances' is your existing list that Register/UnRegister maintains
            var list = new List<PlacementRecord>(instances.Count);

            for (int i = 0; i < instances.Count; i++)
            {
                var inst = instances[i];
                if (inst == null) continue;

                var cfg = inst.GetConfig();
                if (cfg == null) continue; // nothing to identify on reload

                list.Add(new PlacementRecord
                {
                    id = cfg.Id,                  // unique id from the config
                    position = inst.transform.position, // world-space placement
                    rotation = inst.transform.rotation
                });
            }

            return list.ToArray();
        }

        /// <summary>
        /// Recreate registered instances from saved records.
        /// You must provide a resolver that maps config id -> PlaceableItemConfig.
        /// </summary>
        public void FromPlacementRecords( PlacementRecord[] records,
            System.Func<string, PlaceableItemConfig> resolveById)
        {
            if (records == null || records.Length == 0 || resolveById == null) return;

            for (int i = 0; i < records.Length; i++)
            {
                var rec = records[i];
                var cfg = resolveById(rec.id);
                if (cfg == null || cfg.Prefab == null) continue;

                var go = Instantiate(cfg.Prefab, rec.position, rec.rotation);
                var inst = go.GetComponent<PlaceableInstance>();
                if (inst == null) inst = go.AddComponent<PlaceableInstance>();
                inst.Init(cfg);

                // Important: go through Register so your existing event pipeline fires
                Register(inst);
            }
        }

        /// <summary>
        /// Optional convenience if you want to wipe current runtime placements
        /// before loading (for manual reload flows).
        /// </summary>
        public void ClearAllPlacements()
        {
            // copy to avoid mutation while iterating
            var copy = new List<PlaceableInstance>(instances);
            for (int i = 0; i < copy.Count; i++)
            {
                var inst = copy[i];
                if (inst == null) continue;

                // Fire your existing removal event
                UnRegister(inst);

                // Destroy the instance GameObject
                Destroy(inst.gameObject);
            }
        }
    }
}

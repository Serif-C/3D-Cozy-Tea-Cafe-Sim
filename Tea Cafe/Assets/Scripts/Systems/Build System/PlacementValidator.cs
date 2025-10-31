using UnityEngine;

namespace TeaShop.Systems.Building
{
    public class PlacementValidator : MonoBehaviour
    {
        [Header("Collision Rules")]
        [SerializeField] private LayerMask blockedMask;

        [Header("Bounds")]
        [Tooltip("Optional area where building is allowed.")]
        [SerializeField] private Collider allowedArea;

        public bool CanPlaceAt(PlaceableItemConfig item, Vector3 position, Quaternion rotation)
        {
            if (item == null) return false;

            // 1. Check allowed area
            if (allowedArea != null)
            {
                // check if its cointained in the bounding box
                if (!allowedArea.bounds.Contains(position))
                {
                    return false;
                }
            }

            // 2. Overlap test using prefab bounds (assuming collider on root or children exists)
            Bounds b = GetPrefabWorldBounds(item.Prefab, position, rotation);
            Collider[] hits = Physics.OverlapBox(b.center, b.extents, rotation, blockedMask, QueryTriggerInteraction.Ignore);
            if (hits != null && hits.Length > 0)
            {
                return false;
            }

            return true;
        }

        private Bounds GetPrefabWorldBounds(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            Renderer[] rends = prefab.GetComponentsInChildren<Renderer>(true);
            Bounds combined = new Bounds(position, Vector3.zero);
            for (int i = 0; i < rends.Length; i++)
            {
                Bounds wb = rends[i].bounds;
                if (i == 0) combined = wb;
                else combined.Encapsulate(wb);
            }

            return combined;
        }
    }
}

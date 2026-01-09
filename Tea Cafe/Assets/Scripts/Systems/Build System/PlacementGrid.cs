using UnityEngine;

namespace TeaShop.Systems.Building
{
    public class PlacementGrid : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private Transform origin;

        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            Vector3 local = worldPos;
            if (origin != null)
            {
                local = worldPos - origin.position;
            }

            float gx = Mathf.Round(local.x / cellSize) * cellSize;
            float gz = Mathf.Round(local.z / cellSize) * cellSize;

            Vector3 snapped = new Vector3(gx, local.y, gz);
            if (origin != null)
            {
                snapped = origin.position + snapped;
            }

            return snapped;
        }

        public float CellSize { get { return cellSize; } }
    }
}

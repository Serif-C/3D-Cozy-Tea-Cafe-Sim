using UnityEngine;

namespace TeaShop.Systems.Building
{
    public enum TileEdge { Top, Bottom, Left, Right }

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

        public Vector3 SnapToCellCenter(Vector3 worldPos)
        {
            Vector3 local = WorldToLocal(worldPos);

            float gx = Mathf.Round(local.x / cellSize) * cellSize;
            float gz = Mathf.Round(local.z / cellSize) * cellSize;

            Vector3 snappedLocal = new Vector3(gx, local.y, gz);
            return LocalToWorld(snappedLocal);
        }

        public float CellSize { get { return cellSize; } }

        public Vector3 SnapWallToTileEdge(Vector3 worldPos, out TileEdge edge)
        {
            // Step 1: compute the tile center we’re on (forces “on top of tiles” behavior)
            Vector3 tileCenter = SnapToCellCenter(worldPos);

            // Step 2: find offset inside tile in grid-local coordinates
            Vector3 localHit = WorldToLocal(worldPos);
            Vector3 localCenter = WorldToLocal(tileCenter);
            Vector3 d = localHit - localCenter;

            // Step 3: choose which edge (dominant axis)
            // If closer to left/right -> snap to Left/Right edge
            // Else -> snap to Top/Bottom edge
            if (Mathf.Abs(d.x) > Mathf.Abs(d.z))
            {
                if (d.x >= 0f) edge = TileEdge.Right;
                else edge = TileEdge.Left;
            }
            else
            {
                if (d.z >= 0f) edge = TileEdge.Top;
                else edge = TileEdge.Bottom;
            }

            // Step 4: snap to that edge line exactly
            float half = cellSize * 0.5f;
            Vector3 localSnapped = localCenter;

            switch (edge)
            {
                case TileEdge.Right: localSnapped.x += half; break;
                case TileEdge.Left: localSnapped.x -= half; break;
                case TileEdge.Top: localSnapped.z += half; break;
                case TileEdge.Bottom: localSnapped.z -= half; break;
            }

            // Keep Y from original worldPos (or keep tileCenter.y if you prefer)
            localSnapped.y = localHit.y;

            return LocalToWorld(localSnapped);
        }

        public static int BaseYawForEdge(TileEdge edge)
        {
            // Rotation that aligns wall “along the edge”
            // Top/Bottom => wall runs along X (yaw 0/180)
            // Left/Right => wall runs along Z (yaw 90/270)
            return (edge == TileEdge.Left || edge == TileEdge.Right) ? 90 : 0;
        }

        private Vector3 WorldToLocal(Vector3 worldPos)
        {
            return origin != null ? (worldPos - origin.position) : worldPos;
        }

        private Vector3 LocalToWorld(Vector3 localPos)
        {
            return origin != null ? (origin.position + localPos) : localPos;
        }
    }
}

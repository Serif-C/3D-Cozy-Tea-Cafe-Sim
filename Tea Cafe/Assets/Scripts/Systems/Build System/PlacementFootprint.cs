using System.Collections.Generic;
using UnityEngine;

namespace TeaShop.Systems.Building
{
    public static class PlacementFootprint
    {
        /// <summary>
        /// Returns occupied grid cells for a config placed at worldPos/worldRot.
        /// We assume rotation is only yaw (0/90/180/270)
        /// </summary>
        public static List<Vector2Int> GetOccupiedCells(PlaceableItemConfig cfg, Vector3 worldPos, Quaternion worldRot, PlacementGrid grid)
        {
            int w = Mathf.Max(1, cfg.Width);
            int d = Mathf.Max(1, cfg.Depth);

            // if rotated 90/270, swap footprint axes
            int yaw = Mathf.RoundToInt(worldRot.eulerAngles.y) % 360;
            bool swap = (yaw == 90 || yaw == 270);
            int fw = swap ? d : w;
            int fd = swap ? w : d;

            // Convert snapped world position to a cell coordinate (center cell)
            Vector2Int center = WorldToCell(worldPos, grid);

            // Build a rectangle centered on that cell
            // Example: fw =  1 - > x range [0..0], fw = 2 x range [-0..1] etc.
            int minX = center.x - (fw / 2);
            int minZ = center.y - (fd / 2);

            var cells = new List<Vector2Int>(fw * fd);
            for (int x = 0; x < fw; x++)
            {
                for (int z = 0; z < fd; z++)
                {
                    cells.Add(new Vector2Int(minX + x, minZ + z));
                }
            }

            return cells;
        }

        public static Vector2Int WorldToCell(Vector3 worldPos, PlacementGrid grid)
        {
            float size = grid.CellSize;
            Vector3 local = worldPos;

            // PlacementGrid already supports an origin, but it doesn’t expose it.
            // We’ll assume SnapToGrid already aligned this to origin.
            int cx = Mathf.RoundToInt(local.x / size);
            int cz = Mathf.RoundToInt(local.z / size);
            return new Vector2Int(cx, cz);
        }

        public static Vector3 CellToWorld(Vector2Int cell, float y, PlacementGrid grid)
        {
            float size = grid.CellSize;
            return new Vector3(cell.x * size, y, cell.y * size);
        }
    }
}

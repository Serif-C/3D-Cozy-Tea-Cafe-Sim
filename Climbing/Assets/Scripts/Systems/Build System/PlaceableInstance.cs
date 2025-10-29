using UnityEngine;

namespace TeaShop.Systems.Building
{
    // This is a component attached to a spawned item so it can be validated/saved later
    public class PlaceableInstance : MonoBehaviour
    {
        [SerializeField] private PlaceableItemConfig config;
        [SerializeField] private Vector2Int gridSize;

        public void Init(PlaceableItemConfig cfg)
        {
            config = cfg;
            gridSize = new Vector2Int(cfg.Width, cfg.Depth);
        }

        public PlaceableItemConfig GetConfig() { return config; }
        public Vector2Int GetGridSize () {  return gridSize; }
    }
}


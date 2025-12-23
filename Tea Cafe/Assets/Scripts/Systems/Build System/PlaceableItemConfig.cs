using UnityEngine;

namespace TeaShop.Systems.Building
{
    public enum PlaceableCategory 
    { Wall, Table, Chair, Floor, Counter, Stove, BrewingStation, TeaPlant, Decoration }

    [CreateAssetMenu(fileName = "PlaceableItem", menuName = "TeaShop/Building/Placeable Item", order = 0)]
    public class PlaceableItemConfig : ScriptableObject
    {
        [Header("Idendity")]
        [SerializeField] private string id;
        [SerializeField] private PlaceableCategory category;
        [SerializeField] private GameObject prefab;

        [Header("Footprint (grid cells)")]
        [Tooltip("Width in grid cells (x).")]
        [SerializeField] private int width = 1;
        [Tooltip("Depth in grid cells (z).")]
        [SerializeField] private int depth = 1;

        [Header("Economy")]
        [Tooltip("Price in cents.")]
        [SerializeField] private int priceCents = 1000;

        public string Id { get { return id; }}
        public PlaceableCategory Category { get { return category; } }
        public GameObject Prefab { get { return prefab; } }
        public int Width { get { return width; } }
        public int Depth { get { return depth; } }
        public int PriceCents { get { return priceCents; } }

    }
}


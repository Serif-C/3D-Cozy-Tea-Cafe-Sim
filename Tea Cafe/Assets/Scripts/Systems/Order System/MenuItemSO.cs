using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Menu Item")]
public class MenuItemSO : ScriptableObject
{
    public MenuItemID id;
    public string displayName;
    public Sprite icon;
    public MenuCategory category;

    public int basePrice;
}

using System;
using UnityEngine;


[Serializable]
public struct MenuItemID : IEquatable<MenuItemID>
{
    public MenuCategory category;
    public int value; // store enum as int

    public MenuItemID(MenuCategory category, int value)
    {
        this.category = category;
        this.value = value;
    }

    public bool Equals(MenuItemID other) => category == other.category && value == other.value;
    public override int GetHashCode() => ((int)category * 397) ^ value;
}


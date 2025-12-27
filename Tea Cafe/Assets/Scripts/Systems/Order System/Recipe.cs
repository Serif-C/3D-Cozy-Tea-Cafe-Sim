using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum IngredientTag
{
    TeaLeaf,
    Fruit,
    Dairy,
    Liquid
    /// etc...
}

public enum ToolType
{
    None,
    Teapot,
    Kettle,
    /// etc...
}

public enum RecipeOutputKind
{
    Ingredient, // e.g. BoiledWater, Waffle
    MenuItem    // e.g. BlackTea, FruityWaffle (final orders)
}

public enum MenuCategory 
{ 
    Drink,
    Dessert,
    Breakfast,
    Lunch,
    Dinner 
}

[CreateAssetMenu(menuName ="Cafe/Ingredient")]
public class IngredientSO: ScriptableObject
{
    public string id;   // "flour", "sugar", etc...
    public Sprite icon;
    public List<IngredientTag> tags = new();
}

[CreateAssetMenu(menuName ="Cafe/Recipe")]
public class RecipeSO: ScriptableObject
{
    public string id;
    public RecipeOutputKind outputKind;
    public IngredientSO outputIngredient;      // e.g. "BlackTeaLeaf" (if outputKind is Ingredient)\
    public MenuItemID outputMenuItem;
    public ToolType toolType = ToolType.None;
    public int unlockDat = 1;
    public List<IngredientRequirement> requirements = new();
}

public class IngredientRequirement
{
    public IngredientSO specific;               // e.g. "BlackTeaLeaf"  
    public IngredientTag? ingredientTag;        // e.g. "TeaLeaf" (? syntax mean it can hold a null)

    public int amount = 1;

    // If tag-based: require distinct item?
    public bool requiredDistinct = false;       // true for "any 2 different fruit" kind of recipe
    public int distinctCount = 0;               // e.g. 2
}

public struct MenuItemID: IEquatable<MenuItemID>
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

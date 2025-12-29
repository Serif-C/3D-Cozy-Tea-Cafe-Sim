using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum IngredientTag
{
    TeaLeaf,
    Fruit,
    Dairy,
    Liquid,
    Poultry
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

[CreateAssetMenu(menuName ="Cafe/Recipe")]
public class RecipeSO: ScriptableObject
{
    public string id;
    public RecipeOutputKind outputKind;
    public IngredientSO outputIngredient;      // e.g. "BlackTeaLeaf" (if outputKind is Ingredient)\
    public MenuItemID outputMenuItem;
    public ToolType toolType = ToolType.None;
    public int unlockDay = 1;
    [SerializeField] public List<IngredientRequirement> requirements = new List<IngredientRequirement>();
}

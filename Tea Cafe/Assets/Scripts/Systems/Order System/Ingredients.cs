using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string id;   // "flour", "sugar", etc...
    public Sprite icon;
    public List<IngredientTag> tags = new();
}

[System.Serializable]
public class IngredientRequirement
{
    public IngredientSO specific;               // e.g. "BlackTeaLeaf"  
    public IngredientTag? ingredientTag;        // e.g. "TeaLeaf" (? syntax mean it can hold a null)

    public int amount = 1;

    // If tag-based: require distinct item?
    public bool requiredDistinct = false;       // true for "any 2 different fruit" kind of recipe
    public int distinctCount = 0;               // e.g. 2
}
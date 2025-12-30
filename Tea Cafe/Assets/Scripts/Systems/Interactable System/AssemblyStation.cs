using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipePrefabEntry
{
    public RecipeSO recipe;
    public GameObject outputPrefab;
}

public class AssemblyStation : MonoBehaviour
{
    [Header("Station")]
    [SerializeField] private ToolType stationTool = ToolType.None;

    [Header("Slots")]
    [SerializeField] private Transform[] inputSlots;
    [SerializeField] private Transform outputSlot;

    [Header("Recipes")]
    [SerializeField] private List<RecipePrefabEntry> recipes = new();

    [Header("Matching Rules")]
    [Tooltip("If true, station requires ingredients to match recipe exactly (no extras).")]
    [SerializeField] private bool requireExactMatch = true;

    private readonly List<WorldIngredient> placedIngredients = new();
    private GameObject outputItem; // spawned result waiting to be picked up

    public string Prompt
    {
        get
        {
            if (outputItem != null) return "Take Item";
            return "Add Ingredient";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        // While output exists, only allow taking it.
        // Otherwise allow adding if player has something and there's room.
        if (outputItem != null)
        {
            return !player.IsHoldingItem();
        }

        if (player.IsHoldingItem())
        {
            return GetFirstEmptySlot() != null;
        }

        return false;
    }

    public void Interact(PlayerInteractor player)
    {
        // Case 1: Take output item
        if (outputItem != null && !player.isHoldingItem)
        {
            player.PickUp(outputItem);
            outputItem = null;
            return;
        }

        // Case 2: Add ingredient
        if (outputItem != null && player.isHoldingItem)
        {
            // TryAddIngredientFromPlayer(player);
            // TryCraft();
        }
    }

    public Transform GetFirstEmptySlot()
    {
        foreach (var s in inputSlots)
        {
            if (s.childCount == 0)
                return s;
        }
        return null;
    }

    private bool DoesRecipeMatch(RecipeSO recipe)
    {
        // Build counts of placed ingredients (multiset)
        Dictionary<IngredientSO, int> requiredCounts = new();
        foreach (var wi in placedIngredients)
        {
            if (wi == null || wi.ingredient == null) continue;

            if (!requiredCounts.ContainsKey(wi.ingredient))
                requiredCounts[wi.ingredient] = 0;

            requiredCounts[wi.ingredient]++;
        }

        // Check each requirement
        foreach (var req in recipe.requirements)
        {

        }

        return false;
    }
}

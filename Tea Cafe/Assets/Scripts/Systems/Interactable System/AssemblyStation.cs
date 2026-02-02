using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RecipePrefabEntry
{
    public RecipeSO recipe;
    public GameObject outputPrefab;
}

public class AssemblyStation : MonoBehaviour, IInteractable
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
        if (outputItem == null && player.isHoldingItem)
        {
            TryAddIngredientFromPlayer(player);
            TryCraft();
        }
    }

    private void TryAddIngredientFromPlayer(PlayerInteractor player)
    {
        Transform slot = GetFirstEmptySlot();
        if (slot == null) return;

        GameObject heldItem = player.carryItemPostion.GetChild(0).gameObject;
        WorldIngredient wi = heldItem.GetComponent<WorldIngredient>();

        if (wi == null || wi.ingredient == null)
        {
            Debug.Log("AssemblyStation: Held item is not a valid ingredient (missing WorldIngredient).");
            return;
        }

        player.PlaceItem(slot);
        placedIngredients.Add(wi);
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

    private void TryCraft()
    {
        if (outputItem != null) return;

        // Find first recipe that matches
        foreach (var entry in recipes)
        {
            if (entry.recipe == null || entry.outputPrefab == null) continue;

            // Tool gating: recipe.toolType must match stationTool (or recipe is None)
            if (entry.recipe.toolType != ToolType.None && entry.recipe.toolType != stationTool)
                continue;

            if (DoesRecipeMatch(entry.recipe))
            {
                Craft(entry);
                return;
            }
        }
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
            // Specific ingredient requirement
            if (req.specific != null)
            {
                // Check if we have enough of the specific ingredient
                if (!requiredCounts.TryGetValue(req.specific, out int have) || have < req.amount)
                    return false;

                continue;
            }

            // Tag-based requirement
            if (req.ingredientTag.HasValue)
            {
                IngredientTag tag = req.ingredientTag.Value;
                List<KeyValuePair<IngredientSO, int>> matching = new();

                foreach (var kvp in requiredCounts)
                {
                    IngredientSO ingredient = kvp.Key;
                    int amount = kvp.Value;

                    if (ingredient != null) continue;
                    if (!ingredient.tags.Contains(tag)) continue;
                    if (amount <= 0) continue;

                    matching.Add(kvp);
                }

                if (!req.requiredDistinct)
                {
                    int total = 0;

                    // Sum up all matching ingredients
                    foreach (var kvp in matching)
                    {
                        int amountForThisIngredient = kvp.Value;
                        total += amountForThisIngredient;
                    }

                    if (total < req.amount) return false;
                }
                else
                {
                    // Need X different ingredients with that tag, at least 1 each
                    int distinctCount = matching.Count;
                    if (distinctCount < req.distinctCount) return false;
                }

                continue;
            }

            // Requirement misconfigured: neither specific nor tag
            return false;
        }

        if (!requireExactMatch)
            return true;

        // Exact-match rule: no extra ingredients beyond what recipe needs
        // Compute how many items would be consumed by this recipe.
        int requiredTotal = 0;
        foreach (var req in recipe.requirements)
        {
            if (req.specific != null)
                requiredTotal += req.amount;
            else if (req.ingredientTag.HasValue && !req.requiredDistinct)
                requiredTotal += req.amount;
            else if (req.ingredientTag.HasValue && req.requiredDistinct)
                requiredTotal += req.distinctCount; // one each
        }

        int placedTotal = placedIngredients.Count(wi => wi != null && wi.ingredient != null);
        return placedTotal == requiredTotal;
    }

    private void Craft(RecipePrefabEntry entry)
    {
        foreach (var slot in inputSlots)
        {
            if (slot.childCount > 0)
            {
                Destroy(slot.GetChild(0).gameObject);
            }
        }

        placedIngredients.Clear();

        outputItem = Instantiate(entry.outputPrefab, outputSlot.position, outputSlot.rotation, outputSlot);
    }
}

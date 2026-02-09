using UnityEngine;

[CreateAssetMenu(menuName = "Unlockables/Recipe")]
public class RecipeUnlockSO : UnlockableItemSO
{
    [Header("Recipe")]
    public RecipeSO recipe;
}

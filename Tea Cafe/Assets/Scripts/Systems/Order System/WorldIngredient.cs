using UnityEngine;

public class WorldIngredient : MonoBehaviour
{
    public IngredientSO ingredient;

    public Leaf leafObject;

    public DrinkType CheckLeafType()
    {
        return leafObject.GetLeafType();
    }
}

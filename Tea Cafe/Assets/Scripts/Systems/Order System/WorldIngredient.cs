using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorldIngredient : MonoBehaviour
{
    public IngredientSO ingredient;
    public List<IngredientSO> teaLeaves = new();

    private DrinkType leafType;

    private void Start()
    {
        if (!CompareTag("Tea Leaf"))
            return;

        DrinkType leafType = gameObject.GetComponent<Leaf>().GetLeafType();

        foreach (IngredientSO teaLeaf in teaLeaves)
        {
            if (teaLeaf.isTeaLeaf && teaLeaf.leafDrinkType == leafType)
            {
                ingredient = teaLeaf;
                break;
            }
        }
    }
}

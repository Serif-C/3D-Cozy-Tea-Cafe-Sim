using UnityEngine;

public class Leaf : MonoBehaviour
{
    private DrinkType leafType;

    public DrinkType GetLeafType()
    {
        return leafType;
    }

    public void SetLeafType(DrinkType leaf)
    {
        leafType = leaf;
    }
}

using UnityEngine;

public class OrderBubble : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private GameObject orderBubbleVisual;

    public void VisualizeOrder(DrinkType desiredDrink)
    {
        orderBubbleVisual = MenuManager.Instance.GetDesiredDrink(desiredDrink);
        Instantiate(orderBubbleVisual, spawnPoint);
    }
}

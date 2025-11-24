using UnityEngine;

public class OrderBubble : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private SpriteRenderer drinkSpriteToShow;
    private Sprite orderBubbleVisual;

    public void VisualizeOrder(DrinkType desiredDrink)
    {
        orderBubbleVisual = MenuManager.Instance.GetDesiredDrink(desiredDrink);
        drinkSpriteToShow.sprite = orderBubbleVisual;
    }
}

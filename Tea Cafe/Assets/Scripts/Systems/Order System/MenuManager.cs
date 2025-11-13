using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Prefabs of the entire Menu")]
    [SerializeField] private GameObject[] menu;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    public GameObject GetDesiredDrink(DrinkType desireDrink)
    {
        GameObject drink = null;

        if (menu == null) return null; // menu is empty
        
        for (int i = 0; i < menu.Length; i++)
        {
            if (menu[i].gameObject.GetComponent<DrinkItem>().DrinkType == desireDrink)
            {
                drink = menu[i];
                break;
            }
        }

        return drink;
    }
}

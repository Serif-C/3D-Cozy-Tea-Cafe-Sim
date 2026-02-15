using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ShopCategory
{
    Unlocks,
    Teas,
    Decors,
    Appliances
}

public class ShopUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameObject endOfDaySummaryUI;

    [Header("UI")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ShopItemButton itemButtonPrefab;
    [SerializeField] private TMP_Text moneyText;
    private ShopCategory currentCategory;

    [Header("Catalog")]
    [SerializeField] private List<ShopItemDefinition> shopItems;

    private void OnEnable()
    {
        Rebuild();
    }

    public void SetCategory(ShopCategory category)
    {
        currentCategory = category;
        Rebuild();
    }

    public void Rebuild()
    {
        // Clear
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        // Money display
        moneyText.text = $"Money: {PlayerManager.Instance.walletBalance}";

        // Build buttons
        foreach (var item in shopItems)
        {
            if (item.category != currentCategory)
                continue;

            var btn = Instantiate(itemButtonPrefab, itemsContainer);
            btn.Setup(item, shopManager, this);
        }
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
        // optionally show end of day again or proceed next day
        // endOfDaySummaryUI.SetActive(true);
    }
}

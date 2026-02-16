using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject panel;

    [Header("UI")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ShopItemButton itemButtonPrefab;
    [SerializeField] private TMP_Text moneyText;
    private ShopCategory currentCategory;
    [SerializeField] private Button unlocksButton;
    [SerializeField] private Button teasButton;
    [SerializeField] private Button decorsButton;
    [SerializeField] private Sprite UnlockCategorySelected;
    [SerializeField] private Sprite TeasCategorySelected;
    [SerializeField] private Sprite DecorationsCategorySelected;
    private Image selectedCategoryImage;


    [Header("Catalog")]
    [SerializeField] private List<ShopItemDefinition> shopItems;

    private void OnEnable()
    {
        selectedCategoryImage = panel.GetComponent<Image>();
        currentCategory = ShopCategory.Unlocks;
        UpdateTabVisuals();
        Rebuild();
    }

    public void SetCategory(ShopCategory category)
    {
        currentCategory = category;
        UpdateTabVisuals();
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

    private void UpdateTabVisuals()
    {
        unlocksButton.interactable = currentCategory != ShopCategory.Unlocks;
        teasButton.interactable = currentCategory != ShopCategory.Teas;
        decorsButton.interactable = currentCategory != ShopCategory.Decors;

        switch (currentCategory)
        {
            case ShopCategory.Unlocks:
                selectedCategoryImage.sprite = UnlockCategorySelected;
                break;

            case ShopCategory.Teas:
                selectedCategoryImage.sprite = TeasCategorySelected;
                break;

            case ShopCategory.Decors:
                selectedCategoryImage.sprite = DecorationsCategorySelected;
                break;
        }
    }

    // BUTTONS //

    public void SetUnlocksTab()
    {
        SetCategory(ShopCategory.Unlocks);
    }

    public void SetTeasTab()
    {
        SetCategory(ShopCategory.Teas);
    }

    public void SetDecorsTab()
    {
        SetCategory(ShopCategory.Decors);
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
        // optionally show end of day again or proceed next day
        // endOfDaySummaryUI.SetActive(true);
    }
}

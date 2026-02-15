using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{

    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    private ShopItemDefinition item;
    private ShopManager shopManager;
    private ShopUI shopUI;

    public void Setup(ShopItemDefinition item, ShopManager manager, ShopUI ui)
    {
        this.item = item;
        this.shopManager = manager;
        this.shopUI = ui;
        button.image.sprite = item.icon;

        Refresh();
        button.onClick.AddListener(OnClick);
    }

    private void Refresh()
    {
        label.text = $"{item.name} - ${item.cost}";

        // Disable if cannot buy
        button.interactable = shopManager.CanPurchase(item);
    }

    private void OnClick()
    {
        Debug.Log("Clicking works");
        if (shopManager.Purchase(item))
        {
            Debug.Log("Purchase went through");
            // Rebuild to update money + disable unlocked items
            shopUI.Rebuild();
        }
    }
}

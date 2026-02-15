using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private PlayerInventory playerInventory;

    public bool CanPurchase(ShopItemDefinition item)
    {
        if (item == null)
            return false;

        if (PlayerManager.Instance.walletBalance < item.cost)
        {
            Debug.Log("Returned false");
            return false;
        }

        switch (item.itemType)
        {
            case ShopItemType.Unlock:
            {
                if (item.unlockRequirement == null || item.unlockRequirement.unlocksItems == null)
                {
                    Debug.Log("Returned false");
                    return false;
                }

                // Already unlocked check (uses existing system)
                bool allUnlocked = true;
                foreach (var unlockable in item.unlockRequirement.unlocksItems)
                {
                    if (unlockable == null) continue;

                    if (!playerProgress.IsUnlocked(unlockable))
                    {
                        allUnlocked = false;
                        break;
                    }
                }

                if (allUnlocked){Debug.Log("Returned false - all already unlocked"); return false;}

                if (!item.unlockRequirement.unlockViaShopOnly){Debug.Log("Returned false"); return false; /* don’t sell auto-unlock items */}

                Debug.Log("Returned true");
                return true;
            }

            case ShopItemType.InventoryItem:
            {
                // Inventory items only require money
                // Optional For Later: add unlock gating here
                return true;
            }
        }

        Debug.Log("CanPurchase method returned false");
        return false;
    }

    public bool Purchase(ShopItemDefinition item)
    {
        if (!CanPurchase(item))
            return false;

        PlayerManager.Instance.SetWalletBalance(PlayerManager.Instance.walletBalance - item.cost);

        switch (item.itemType)
        {
            // Unlock via PlayerProgress
            case ShopItemType.Unlock:
                foreach (var unlockable in item.unlockRequirement.unlocksItems)
                {
                    if (unlockable == null) continue;

                    if (!playerProgress.IsUnlocked(unlockable))
                        playerProgress.Unlock(unlockable);
                }
                break;

            case ShopItemType.InventoryItem:
                if (item.inventoryPrefab != null)
                {
                    playerInventory.Add(item.inventoryPrefab);
                }
                break;
        }

        return true;
    }

    public PlayerProgress GetPlayerProgress() => playerProgress;
}
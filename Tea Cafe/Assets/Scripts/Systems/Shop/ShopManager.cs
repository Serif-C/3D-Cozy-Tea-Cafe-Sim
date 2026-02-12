using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress playerProgress;

    public bool CanPurchase(ShopItemDefinition item)
    {
        if (item == null || item.unlockRequirement == null || item.unlockRequirement.unlocksItems == null)
        {
            Debug.Log("Returned false");
            return false;
        }
        // Money check
        if (PlayerManager.Instance.walletBalance < item.cost)
        {
            Debug.Log("Returned false");
            return false;
        }
        // Already unlocked check (uses YOUR existing system)
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

        if (allUnlocked)
        {
            Debug.Log("Returned false - all already unlocked");
            return false;
        }

        if (!item.unlockRequirement.unlockViaShopOnly)
        {
            Debug.Log("Returned false");
            return false; // don’t sell auto-unlock items
        }
        //// Optional: requirement gating (rank/rep/happy, etc.)
        //// If Happy Customers isn't implemented yet, this will still work if Meets() ignores it or returns true.
        //if (!playerProgress.Meets(item.unlockRequirement))
        //    return false;
        Debug.Log("Returned true");
        return true;
    }

    public bool Purchase(ShopItemDefinition item)
    {
        if (!CanPurchase(item))
            return false;

        // Spend money (uses your existing PlayerManager API)
        PlayerManager.Instance.SetWalletBalance(PlayerManager.Instance.walletBalance - item.cost);

        // Unlock via PlayerProgress (NOT a singleton)
        foreach (var unlockable in item.unlockRequirement.unlocksItems)
        {
            if (unlockable == null) continue;

            if (!playerProgress.IsUnlocked(unlockable))
                playerProgress.Unlock(unlockable);
        }

        return true;
    }

    public PlayerProgress GetPlayerProgress() => playerProgress;
}
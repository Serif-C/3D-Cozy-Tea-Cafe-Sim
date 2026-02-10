using System.Collections.Generic;
using UnityEngine;

//public class ShopManager
//{
//    private readonly PlayerProgress progress;

//    public ShopManager(PlayerProgress progress)
//    {
//        this.progress = progress;
//    }

//    public int CurrentMoney => PlayerManager.Instance != null
//        ? PlayerManager.Instance.walletBalance
//        : 0;

//    public bool CanPurchase(ShopItemDefinition item)
//    {
//        if (item == null || item.unlockRequirement == null || item.unlockRequirement.unlocksItem == null)
//            return false;

//        // Already unlocked?
//        if (progress.IsUnlocked(item.unlockRequirement.unlocksItem))
//            return false;

//        // Can afford?
//        if (CurrentMoney < item.cost)
//            return false;

//        // Optional: respect rank/reputation gating for whether it even appears purchasable
//        // If you want shop items ALWAYS buyable regardless of rank/rep, remove this check.
//        if (!progress.Meets(item.unlockRequirement))
//            return false;

//        return true;
//    }

//    public bool Purchase(ShopItemDefinition item)
//    {
//        if (!CanPurchase(item))
//            return false;

//        // Spend money (your existing system)
//        PlayerManager.Instance.SetWalletBalance(CurrentMoney - item.cost);

//        // Unlock via your existing system
//        progress.Unlock(item.unlockRequirement.unlocksItem);

//        return true;
//    }
//}
public class ShopManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress playerProgress;

    public bool CanPurchase(ShopItemDefinition item)
    {
        if (item == null || item.unlockRequirement == null || item.unlockRequirement.unlocksItem == null)
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
        if (playerProgress.IsUnlocked(item.unlockRequirement.unlocksItem))
        {
            Debug.Log("Returned false");
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
        playerProgress.Unlock(item.unlockRequirement.unlocksItem);

        return true;
    }

    public PlayerProgress GetPlayerProgress() => playerProgress;
}
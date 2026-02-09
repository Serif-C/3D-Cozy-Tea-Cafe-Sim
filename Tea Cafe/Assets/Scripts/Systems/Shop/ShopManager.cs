using System.Collections.Generic;
using UnityEngine;

public class ShopManager
{
    private readonly PlayerProgress progress;

    public ShopManager(PlayerProgress progress)
    {
        this.progress = progress;
    }

    public int CurrentMoney => PlayerManager.Instance != null
        ? PlayerManager.Instance.walletBalance
        : 0;

    public bool CanPurchase(ShopItemDefinition item)
    {
        if (item == null || item.unlockRequirement == null || item.unlockRequirement.unlocksItem == null)
            return false;

        // Already unlocked?
        if (progress.IsUnlocked(item.unlockRequirement.unlocksItem))
            return false;

        // Can afford?
        if (CurrentMoney < item.cost)
            return false;

        // Optional: respect rank/reputation gating for whether it even appears purchasable
        // If you want shop items ALWAYS buyable regardless of rank/rep, remove this check.
        if (!progress.Meets(item.unlockRequirement))
            return false;

        return true;
    }

    public bool Purchase(ShopItemDefinition item)
    {
        if (!CanPurchase(item))
            return false;

        // Spend money (your existing system)
        PlayerManager.Instance.SetWalletBalance(CurrentMoney - item.cost);

        // Unlock via your existing system
        progress.Unlock(item.unlockRequirement.unlocksItem);

        return true;
    }
}

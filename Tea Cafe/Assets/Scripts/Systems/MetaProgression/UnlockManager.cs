using UnityEngine;

public enum UnlockStatType
{
    CafeRank,
    CafeReputation
}

public class UnlockManager : MonoBehaviour
{
    private PlayerProgress progress;

    public UnlockManager(PlayerProgress progress)
    {
        this.progress = progress;
    }

    public bool IsUnlocked(UnlockDefinition def)
    {
        foreach (var cond in def.conditions)
        {
            if (!CheckCondition(cond))
                return false;
        }
        return true;
    }

    private bool CheckCondition(UnlockCondition cond)
    {
        return cond.stat switch
        {
            UnlockStatType.CafeRank => progress.cafeRank.Level >= cond.requiredValue,
            UnlockStatType.CafeReputation => progress.cafeReputation.Value >= cond.requiredValue,
            _ => false
        };
    }
}

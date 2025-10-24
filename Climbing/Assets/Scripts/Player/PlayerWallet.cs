using UnityEngine;

public class PlayerWallet : MonoBehaviour, ICoinCollector
{
    public void AddCoinOnWallet(int amount)
    {
        PlayerManager.Instance.SetCountAmount(amount);
    }
}

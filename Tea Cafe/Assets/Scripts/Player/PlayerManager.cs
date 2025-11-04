using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public int walletBalance = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCountAmount(int amount)
    {
        walletBalance += amount;
    }

    public void SetWalletBalance(int balance)
    {
        walletBalance = balance;
    }

    public void ResetWallet()
    {
        walletBalance = 0;
        SaveWallet();
    }

    private void LoadWallet()
    {
        PlayerSaveData data = SaveSystem.Load();

        if (data != null)
        {
            // migrate if needed
            if (data.version < 1)
            {
                // migration logic if needed
                data.version = 1;
            }

            walletBalance = data.walletBalance;
        }
        else
        {
            walletBalance = 0;
            // create initial save
            SaveWallet();
        }
    }

    private void SaveWallet()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.version = 1;
        data.walletBalance = walletBalance;
        SaveSystem.Save(data);
    }

    private void OnApplicationQuit()
    {
        SaveWallet();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveWallet();
        }    
    }
}

using UnityEngine;

public class PlayerManager : MonoBehaviour, IMoneyEarnedSource
{
    public static PlayerManager Instance;
    public int walletBalance = 0;
    private bool isQuitting;

    public event System.Action<int> MoneyEarned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadWallet();
    }

    public void SetCountAmount(int amount)
    {
        walletBalance += amount;
        MoneyEarned?.Invoke(amount);
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
        // Merge with existing save so we DON'T lose placements
        var data = SaveSystem.Load() ?? new PlayerSaveData();
        data.version = Mathf.Max(data.version, 1);
        data.walletBalance = walletBalance;
        SaveSystem.Save(data);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        SaveWallet();
    }

    private void OnDestroy()
    {
        if (Instance == this && !isQuitting && Application.isPlaying)
            SaveWallet();
    }
}

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class MenuEntry
{
    public GameObject menuItem;
    public Sprite icon;
    public DrinkType drinkType;
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private List<MenuEntry> menuEntries;
    private Dictionary<DrinkType, MenuEntry> entriesByType;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;

        entriesByType = new Dictionary<DrinkType, MenuEntry>();

        foreach (MenuEntry entry in menuEntries)
        {
            if (entriesByType.ContainsKey(entry.drinkType))
            {
                Debug.LogWarning($"MenuManager: duplicate drink type {entry.drinkType}");
                continue;
            }
            else
            {
                entriesByType.Add(entry.drinkType, entry);
            }
        }
    }

    public Sprite GetDesiredDrink(DrinkType desiredDrink)
    {
        if (entriesByType != null && entriesByType.TryGetValue(desiredDrink, out MenuEntry entry))
        {
            return entry.icon;
        }

        return null;
    }
}

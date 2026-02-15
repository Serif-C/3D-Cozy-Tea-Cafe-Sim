using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Progression/Player Inventory")]
public class PlayerInventory : ScriptableObject
{
    private List<GameObject> ownedPrefabs = new();

    public void Add(GameObject prefab)
    {
        if (prefab == null) return;

        ownedPrefabs.Add(prefab);
        Debug.Log($"Added to inventory: {prefab.name}");
    }    

    public IReadOnlyList<GameObject> GetAll()
    {
        return ownedPrefabs;
    }

    public void Clear()
    {
        ownedPrefabs.Clear();
    }
}

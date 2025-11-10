using UnityEngine;

[System.Serializable]
public struct PlacementRecord
{
    public string id;        // PlaceableItemConfig.Id
    public Vector3 position; // world position
    public Quaternion rotation;
}

[System.Serializable]
public class PlayerSaveData
{
    public int version = 1;
    public int walletBalance;

    // more fields later
    public PlacementRecord[] placements; // null/empty if nothing placed
}

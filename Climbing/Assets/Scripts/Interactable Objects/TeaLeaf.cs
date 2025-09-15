using UnityEngine;

public class TeaLeaf : MonoBehaviour, IInteractable
{
    [Header("Tea Leaf Settings")]
    [SerializeField] private GameObject teaLeaftPrefab;
    [SerializeField] private string teaLeafName;
    [SerializeField] private Transform spawnPoint;

    public string Prompt
    {
        get
        {
            return "";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        if (player.IsHoldingItem()) return false;
        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        Debug.Log("Tea Flower: Player takes a Tea Leaf!");
        GameObject item = Instantiate(teaLeaftPrefab, spawnPoint.position, Quaternion.identity);
        player.PickUp(item);
    }
}

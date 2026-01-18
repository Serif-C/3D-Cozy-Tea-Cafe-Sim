using UnityEngine;

public class TeaLeaf : MonoBehaviour, IInteractable
{
    [Header("Tea Leaf Settings")]
    [SerializeField] private GameObject teaLeaftPrefab;
    //[SerializeField] private Transform teaLeafSpawnPoint;
    public DrinkType leafType;  // The type of drink the plant produces

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
        //GameObject item = Instantiate(teaLeaftPrefab, teaLeafSpawnPoint.position, Quaternion.identity);
        GameObject item = Instantiate(teaLeaftPrefab);
        item.GetComponent<Leaf>().SetLeafType(leafType);
        player.PickUp(item);
    }

    public GameObject GetTeaPrefab()
    {
        GameObject item = Instantiate (teaLeaftPrefab);
        item.GetComponent <Leaf>().SetLeafType(leafType);
        return item;
    }
}
 
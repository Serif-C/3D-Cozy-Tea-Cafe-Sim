using UnityEngine;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [Header("Chopping Board Settings")]
    [SerializeField] private float choppingTime = 3f;
    private float timer = 0f;
    private bool isChopping = false;
    private bool hasFinishedChopping = false;

    [Header("Visuals")]
    [SerializeField] private GameObject choppingItemPrefab;
    [SerializeField] private Transform spawnPoint;

    public string Prompt
    {
        get
        {
            if (!isChopping && !hasFinishedChopping)
            {
                return "Start Chopping";
            }

            if (isChopping)
            {
                return "Chopping... ";
            }

            if (hasFinishedChopping)
            {
                return "Take chopped Tea Leaves";
            }

            return "";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        if (isChopping) return false;
        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        if (!isChopping && !hasFinishedChopping)
        {
            Debug.Log("ChoppingBoard: Started Chopping Tea Leaves!");
            isChopping = true;
            timer = choppingTime;
        }

        else if (hasFinishedChopping)
        {
            Debug.Log("ChoppingBoard: Player takes chopped Tea Leaves");
            GameObject item = Instantiate(choppingItemPrefab, spawnPoint.position, Quaternion.identity);
            player.PickUp(item);
            hasFinishedChopping = false;
        }
    }

    private void Update()
    {
        if (isChopping)
        {
            timer -= Time.deltaTime;

            if(timer <= 0f)
            {
                isChopping = false;
                hasFinishedChopping = true;
                Debug.Log("Chopping Board: Tea Leaves has been Chopped!");
            }
        }
    }
}

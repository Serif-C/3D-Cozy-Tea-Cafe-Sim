using UnityEngine;

// Serving Counter where the finished item gets sent for plating
public class ServingCounter : MonoBehaviour, IInteractable
{
    [Header("Plating Item Settings")]
    [SerializeField] private float platingTime = 3f;
    private float timer = 0f;
    private bool isPlating;
    private bool isReadyToServe;

    [Header("Visuals")]
    [SerializeField] private GameObject platedItemPrefab;
    [SerializeField] private Transform spawnPoint;

    public string Prompt
    {
        get
        {
            if (!isPlating && !isReadyToServe)
            {
                return "Start plating";
            }

            if (isPlating)
            {
                return "Plating... ";
            }

            if(isReadyToServe)
            {
                return "Serve Item";
            }

            return "";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        if (isPlating) return false;
        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        if (!isPlating && !isReadyToServe)
        {
            Debug.Log("Serving Counter: Item plated");
            isPlating = true;
            timer = platingTime;
        }

        else if (isReadyToServe)
        {
            Debug.Log("Serving Counter: Player takes plated item");
            GameObject item = Instantiate(platedItemPrefab, spawnPoint.position, Quaternion.identity);
            player.PickUp(item); 
            isReadyToServe = false;
        }
    }

    private void Update()
    {
        if (isPlating)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isPlating = false;
                isReadyToServe = true;
                //fireEffect?.Stop();
                Debug.Log("Stove: Food is ready!");
            }
        }
    }

}

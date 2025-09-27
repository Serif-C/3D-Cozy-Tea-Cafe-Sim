using NUnit.Framework;
using UnityEngine;

public class BrewingStation : MonoBehaviour, IInteractable
{
    [Header("Brewing Settings")]
    [SerializeField] private float brewingTime = 5f;
    private float timer = 0f;
    private bool isBrewing = false;
    private bool isFinishedBrewing = false;
    private GameObject storedItem; // what's on the station right now

    [Header("Visuals")]
    [SerializeField] private GameObject brewedTeaPrefab;
    //[SerializeField] private Tea type;    // shows the tea type later
    [SerializeField] private Transform spawnPoint;  // where brewing prefab spawns

    public string Prompt
    {
        get
        {
            if (!isBrewing && !isFinishedBrewing)
            {
                return "Start Brewing";
            }

            if (isBrewing)
            {
                return "Brewing... ";
            }

            if (isFinishedBrewing)
            {
                return "Take Tea";
            }

            return "";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        // While brewing, prevent interaction (except taking finished item)
        if (isBrewing) return false; 
        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        // Case 1: place boiled water (when empty)
        if (!isBrewing && !isFinishedBrewing && storedItem == null && player.IsHoldingItem())
        {
            // Only accept boiled water
            if (player.HeldItemHasTag("Boiled Water"))
            {
                player.PlaceItem(spawnPoint);

                if (spawnPoint.childCount > 0)
                    storedItem = spawnPoint.GetChild(0).gameObject;
                else
                    storedItem = null;

                //storedItem = spawnPoint.childCount > 0 ? spawnPoint.GetChild(0).gameObject : null;

                // Start brewing immediately after correct placement
                isBrewing = true;
                timer = brewingTime;
                Debug.Log("Brewing Station: Boiled water placed. Brewing started!");
            }
            else
            {
                Debug.Log("Brewing Station: This station only accepts Boiled Water.");
            }
            return;
        }

        // Case 2: take finished tea
        if (isFinishedBrewing && !player.IsHoldingItem())
        {
            if (storedItem != null) // storedItem is the finished tea now
            {
                player.PickUp(storedItem);
                storedItem = null;
                isFinishedBrewing = false;
                Debug.Log("Brewing Station: Player took tea.");
            }
            return;
        }
    }

    public void Update()
    {
        if (isBrewing)
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                isBrewing = false;
                isFinishedBrewing = true;

                // Replace boiled water with brewed tea on the socket
                if (storedItem != null) Destroy(storedItem);
                storedItem = Instantiate(brewedTeaPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                Debug.Log("Brewing Station: Tea is ready!");
            }
        }
    }
}

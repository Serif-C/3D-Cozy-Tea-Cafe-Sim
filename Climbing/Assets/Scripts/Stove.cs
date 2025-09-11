using UnityEngine;

public class Stove : MonoBehaviour, IInteractable
{
    [Header("Cooking Settings")]
    [SerializeField] private float cookTime = 3f;
    private float timer = 0f;             // tracks cooking progress
    private bool isCooking = false;       // is stove currently active?
    private bool hasFinishedItem = false; // is something ready to pick up?

    [Header("Visuals")]
    [SerializeField] private GameObject cookedItemPrefab;   // what gets produced
    [SerializeField] private Transform spawnPoint;          // where item spawns
    [SerializeField] private ParticleSystem fireEffect;

    // Interface property: what text to show player
    public string Prompt
    {
        get
        {
            if (!isCooking && !hasFinishedItem)
            {
                return "Start Cooking";
            }

            if (isCooking)
            {
                return "Cooking... ";
            }

            if (hasFinishedItem)
            {
                return "Take Food";
            }

            return "";
        }
    }

    // Interface method: can player interact now?
    public bool CanInteract(PlayerInteractor player)
    {
        // While cooking, prevent interaction (except taking finished item)
        if (isCooking) return false;
        return true;
    }

    // Interface method: what happens when interact
    public void Interact(PlayerInteractor player)
    {
        // Case 1: start cooking
        if (!isCooking && !hasFinishedItem)
        {
            Debug.Log("Stove: Started cooking!");
            isCooking = true;
            timer = cookTime;
            fireEffect?.Play(); // start flames if assigned
        }
        // Case 2: take finished item
        else if (hasFinishedItem)
        {
            Debug.Log("Stove: Player takes cooked food!");
            GameObject item = Instantiate(cookedItemPrefab, spawnPoint.position, Quaternion.identity);
            //player.PickUp(item); // give it to the player
            hasFinishedItem = false;
        }
    }

    private void Update()
    {
        if (isCooking)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isCooking = false;
                hasFinishedItem = true;
                fireEffect?.Stop();
                Debug.Log("Stove: Food is ready!");
            }
        }
    }
}

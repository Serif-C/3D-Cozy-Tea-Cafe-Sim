using UnityEngine;

// Stove is for boiling water
public class Stove : MonoBehaviour, IInteractable
{
    [Header("Cooking Settings")]
    [SerializeField] private float cookTime = 3f;
    private float timer = 0f;             
    private bool isBoiling = false;       // is stove currently active?
    private bool hasFinishedItem = false; // is something ready to pick up?

    [Header("Visuals")]
    [SerializeField] private GameObject cookedItemPrefab;   
    [SerializeField] private Transform spawnPoint;          // where item spawns
    //[SerializeField] private ParticleSystem fireEffect;

    // Interface property: what text to show player
    public string Prompt
    {
        get
        {
            if (!isBoiling && !hasFinishedItem)
            {
                return "Start Boiling";
            }

            if (isBoiling)
            {
                return "Boiling... ";
            }

            if (hasFinishedItem)
            {
                return "Take Boiled Water";
            }

            return "";
        }
    }

    // Interface method: can player interact now?
    public bool CanInteract(PlayerInteractor player)
    {
        // While boiling, prevent interaction (except taking finished item)
        if (isBoiling) return false;
        return true;
    }

    // Interface method: what happens when interact
    public void Interact(PlayerInteractor player)
    {
        // Case 1: start boiling
        if (!isBoiling && !hasFinishedItem)
        {
            Debug.Log("Stove: Started Boiling Water!");
            isBoiling = true;
            timer = cookTime;
            //fireEffect?.Play(); // start flames if assigned
        }
        // Case 2: take finished item
        else if (hasFinishedItem && !player.IsHoldingItem())
        {
            Debug.Log("Stove: Player takes Boiled Water!");
            GameObject item = Instantiate(cookedItemPrefab, spawnPoint.position, Quaternion.identity);
            player.PickUp(item);
            hasFinishedItem = false;
        }
    }

    private void Update()
    {
        if (isBoiling)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isBoiling = false;
                hasFinishedItem = true;
                //fireEffect?.Stop();
                Debug.Log("Stove: Boiled Water is ready!");
            }
        }
    }
}

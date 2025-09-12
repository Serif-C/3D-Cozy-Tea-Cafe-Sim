using NUnit.Framework;
using UnityEngine;

public class BrewingStation : MonoBehaviour, IInteractable
{
    [Header("Brewing Settings")]
    [SerializeField] private float brewingTime = 5f;
    private float timer = 0f;
    private bool isBrewing = false;
    private bool isFinishedBrewing = false;

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
        // Case 1: start cooking
        if (!isBrewing && !isFinishedBrewing)
        {
            Debug.Log("Brewing Station: Started Boiling Water!");
            isBrewing = true;
            timer = brewingTime;
            //fireEffect?.Play(); // start flames if assigned
        }
        // Case 2: take finished item
        else if (isFinishedBrewing)
        {
            Debug.Log("Brewing Station: Player takes Boiled Water!");
            GameObject item = Instantiate(brewedTeaPrefab, spawnPoint.position, Quaternion.identity);
            player.PickUp(item);
            isFinishedBrewing = false;
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
                Debug.Log("Brewing Station: Tea is ready!");
            }
        }
    }
}

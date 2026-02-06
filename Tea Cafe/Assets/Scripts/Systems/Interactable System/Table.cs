using System;
using UnityEngine;

public class Table : MonoBehaviour, IInteractable, IHasProgress
{
    [Header("Table Settings")]
    [SerializeField] private float customerSeatTime = 10f;  // How long the customer takes to finish
    [SerializeField] private float cleaningTime = 3f;
    [SerializeField] private bool isServed = false;
    private float timer = 0f;
    private bool isCleaning = false;
    private bool requiresCleaning = false;
    private bool hasFinishedCleaning = false;
    private GameObject storedItem;
    [SerializeField] private DrinkItem servedDrinkMeta;

    [Header("Table Occupation Settings")]
    private bool isTableOccupied = false;
    public void SetOccupiedValue(bool value) { isTableOccupied = value; }
    public bool IsTableOccupied() { return isTableOccupied; }


    [Header("Visuals")]
    [SerializeField] private Transform spawnPoint;

    public event Action<float, bool> OnProgressChanged;

    // CustomerBrain uses this to check:
    public bool HasDrinkOfType(DrinkType type)
    {
        // Served, have a tea, and matches ID and Drink
        return isServed
            && spawnPoint != null
            && spawnPoint.childCount > 1
            && servedDrinkMeta != null
            && servedDrinkMeta.DrinkType == type;
    }

    // Utility so we don't duplicate this pattern
    private bool servedItemExists()
    {
                                                // Completion bar occupies index = 0
        return isServed && spawnPoint != null && spawnPoint.childCount > 1 && servedDrinkMeta != null;    
    }
    
    // Expose the raw meta if ever needed
    public bool TryGetServedDrink(out DrinkItem meta)
    {
        meta = servedDrinkMeta;
        return isServed && servedItemExists() && servedDrinkMeta != null;
    }

    public float Progress01
    {
        get
        {
            if (isCleaning)
            {
                float normalized = 1f - (timer / cleaningTime);
                return Mathf.Clamp01(normalized);
            }
            else
            {
                if (hasFinishedCleaning)
                {
                    return 1f;
                }
                else
                {
                    return 0f;
                }
            }
        }
    }

    public bool ShowProgress
    {
        get
        {
            if (isCleaning)
                return true;
            else
                return false;
        }
    }

    public string Prompt
    {
        get
        {
            if (isServed)
            {
                return "Customer served";
            }

            if (requiresCleaning)
            {
                return "Cleaning table";
            }

            if (isCleaning)
            {
                return "Cleaning... ";
            }

            return "";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        // can interact if customer hasnt been served or when table needs to be cleaned
        if (!isServed || requiresCleaning)
        {
            return true;
        }

        return false;
    }

    public void Interact(PlayerInteractor player)
    {
        // Case 1: Customer hasn't been served
        if (!isServed && !isCleaning && !requiresCleaning)
        {
            if (player.HeldItemHasTag("Tea"))
            {
                player.PlaceItem(spawnPoint);

                if (spawnPoint.childCount > 1)
                {
                    storedItem = spawnPoint.GetChild(spawnPoint.childCount - 1).gameObject;
                    // read DrinkItem meta from the placed object
                    servedDrinkMeta = storedItem.GetComponent<DrinkItem>();
                }
                else
                {
                    storedItem = null;
                    servedDrinkMeta = null;
                }

                isServed = true;
                timer = customerSeatTime;

                //Debug.Log("Table: Customer has been served!");
            }
            else
            {
                Debug.Log("Table: Only accepts Tea");
            }
        }

        // Case 2: Customer is done 
        else if (isServed && requiresCleaning && !isCleaning)
        {
            timer = cleaningTime;
            isCleaning = true;
            hasFinishedCleaning = false;
            Debug.Log("Table: Table is being Cleaned");
        }

        // Case 3: Table is fresh (i.e., has been cleaned or empty)
        else
        {
            Debug.Log("There is nothing to Clean");
        }
    }

    public void Update()
    {
        if (isServed)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                // Destroy finished item for now
                Destroy(storedItem);
                requiresCleaning = true;
            }
        }

        if (isCleaning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isCleaning = false;
                requiresCleaning = false;
                hasFinishedCleaning = true;
                isServed = false;

                // drop references
                servedDrinkMeta = null;
                storedItem = null;

                RaiseProgressChanged();
                //Debug.Log("Finished cleaning");
            }
            else
            {
                RaiseProgressChanged();
            }
        }
    }
    
    private void RaiseProgressChanged()
    {
        if (OnProgressChanged != null)
        {
            float progressValue = Progress01;
            bool shouldShow = ShowProgress;

            // Call every method subscribed to this event, passing the values
            OnProgressChanged.Invoke(progressValue, shouldShow);
        }
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoint;
    }
}

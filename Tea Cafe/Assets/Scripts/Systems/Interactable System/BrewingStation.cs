using System;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class IngredientRequirement
{
    public string requiredTag;
    public int requiredAmount = 1;

    [HideInInspector] public int currentAmount;
}

public class BrewingStation : MonoBehaviour, IInteractable, IHasProgress
{
    [Header("Brewing Settings")]
    [SerializeField] private float brewingTime = 5f;
    private float timer = 0f;
    private bool isBrewing = false;
    private bool isFinishedBrewing = false;
    private GameObject storedItem; // what's on the station right now
    private bool hasTeaLeaf = false;
    private bool hasBoiledWater = false;

    [Header("Visuals")]
    [SerializeField] private GameObject[] brewedTeaPrefab;
    //[SerializeField] private Tea type;    // shows the tea type later
    [SerializeField] private Transform spawnPoint;  // where brewing prefab spawns

    // IHasProgress attribute
    public event Action<float, bool> OnProgressChanged;

    [Header("Recipe Settings")]
    [SerializeField] private IngredientRequirement[] requirements;
    private DrinkType leafType;

    public float Progress01
    {
        get
        {
            if (isBrewing)
            {
                float normalized = 1f - (timer / brewingTime);
                return Mathf.Clamp01(normalized);
            }
            else
            {
                if (isFinishedBrewing)
                    return 1f;
                else
                    return 0f;
            }
        }
    }

    public bool ShowProgress
    {
        get
        {
            if (isBrewing)
                return true;
            else
                return false;
        }
    }

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

    //public void Interact(PlayerInteractor player)
    //{
    //    if (!isBrewing && !isFinishedBrewing && player.IsHoldingItem())
    //    {
    //        // Accepts Tea leaf
    //        if (player.HeldItemHasTag("Tea Leaf"))
    //        {
    //            leafType = player.gameObject.GetComponentInChildren<Leaf>().GetLeafType();
    //            player.PlaceItem(spawnPoint);

    //            // To hide tea leaf as it doesnt need to be placed in the game scene
    //            hasTeaLeaf = true;
    //            for (int i = 0; i < spawnPoint.childCount; i++)
    //            {
    //                if (spawnPoint.GetChild(i).CompareTag("Tea Leaf"))
    //                {
    //                    Destroy(spawnPoint.GetChild(i).gameObject);
    //                }
    //            }

    //            Debug.Log("Recieved Tea Leaf");

    //            // Start brewing immediately after correct placement
    //            if (hasTeaLeaf && hasBoiledWater)
    //            {
    //                isBrewing = true;
    //                timer = brewingTime;
    //                Debug.Log("Brewing Station: Boiled water and Tea Leaf placed. Brewing started!");
    //            }
    //        }

    //        // Accepts Boiled Water
    //        else if (player.HeldItemHasTag("Boiled Water"))
    //        {
    //            player.PlaceItem(spawnPoint);

    //            StoreItemAsNextChild();

    //            hasBoiledWater = true;
    //            Debug.Log("Recieved Boiled Water");
                
    //            //Start brewing immediately after correct placement
    //            if (hasTeaLeaf && hasBoiledWater)
    //            {
    //                isBrewing = true;
    //                timer = brewingTime;
    //                Debug.Log("Brewing Station: Boiled water and Tea Leaf placed. Brewing started!");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("Brewing Station: This station only accepts Boiled Water and Tea Leaf.");
    //        }
    //        return;
    //    }

    //    // Case 2: take finished tea
    //    if (isFinishedBrewing && !player.IsHoldingItem())
    //    {
    //        if (storedItem != null) // storedItem is the finished tea now
    //        {
    //            player.PickUp(storedItem);
    //            storedItem = null;
    //            isFinishedBrewing = false;
    //            hasTeaLeaf = false;
    //            hasBoiledWater = false;
    //            Debug.Log("Brewing Station: Player took tea.");
    //        }
    //        return;
    //    }
    //}

    public void Interact(PlayerInteractor player)
    {
        // Case 1: add ingredients (only if not brewing and not finished)
        if (!isBrewing && !isFinishedBrewing && player.IsHoldingItem())
        {
            bool accepted = TryAcceptingIngredient(player);

            if (accepted && AllRequirementsMet())
            {
                // All prerequisites satisfied -> start brewing
                isBrewing = true;
                timer = brewingTime;
                Debug.Log("Brewing Station: All ingredients placed. Brewing started!");
            }

            return;
        }

        // Case 2: take finished tea
        if (isFinishedBrewing && !player.IsHoldingItem())
        {
            if (storedItem != null)
            {
                player.PickUp(storedItem);
                storedItem = null;
                isFinishedBrewing = false;

                ResetRequirements();   // ready for next recipe
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
            if (timer <= 0f)
            {
                isBrewing = false;
                isFinishedBrewing = true;

                // Replace boiled water with brewed tea on the socket
                if (storedItem != null)
                {
                    Destroy(storedItem);
                }

                for (int i = 0; i < brewedTeaPrefab.Length; i++)
                {
                    if (leafType == brewedTeaPrefab[i].gameObject.GetComponent<DrinkItem>().DrinkType)
                    {
                        storedItem = Instantiate(brewedTeaPrefab[i], spawnPoint.position, spawnPoint.rotation, spawnPoint);
                    }
                }

                RaiseProgressChanged();
                Debug.Log("Brewing Station: Tea is ready!");
            }
            else
            {
                RaiseProgressChanged();
            }
        }
    }

    private bool AllRequirementsMet()
    {
        foreach (var req in requirements)
        {
            if (req.currentAmount < req.requiredAmount)
            {
                return false;
            }
        }

        return true;
    }

    private void ResetRequirements()
    {
        foreach (var req in requirements)
        {
            req.currentAmount = 0;
        }
    }

    private bool TryAcceptingIngredient(PlayerInteractor player)
    {
        if (!player.isHoldingItem)
        {
            return false;
        }

        GameObject held = player.carryItemPostion.GetChild(0).gameObject;

        foreach (var req in requirements)
        {
            // skip requirements that are already full
            if (req.currentAmount >= req.requiredAmount)
                continue;

            if (!held.CompareTag(req.requiredTag))
                continue;

            if (req.requiredTag == "Tea Leaf")
            {
                // Capture which tea leaf so we know which brewed tea to spawn
                leafType = held.GetComponent<Leaf>().GetLeafType();

                // Leaf is not needed visually
                Destroy(held);

                player.SetIsHoldingItem(false);
            }
            else
            {
                // Boiled water sits on the spawn point
                player.PlaceItem(spawnPoint);
                StoreItemAsNextChild();
            }

            req.currentAmount++;
            return true;
        }

        Debug.Log("Brewing Station: This ingredient isn't needed for this recipe.");
        return false;
    }

    private void StoreItemAsNextChild()
    {
        if (spawnPoint.childCount > 0)
            storedItem = spawnPoint.GetChild(spawnPoint.childCount - 1).gameObject;
        else
            storedItem = null;
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
}

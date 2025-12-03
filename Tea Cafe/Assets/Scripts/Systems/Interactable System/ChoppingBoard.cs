using System;
using UnityEngine;

[Serializable]
public class ChoppingBoardRequirements
{
    public string[] acceptableTags;
    public int requiredAmount = 1;

    [HideInInspector] public int currentAmount;
}

public class ChoppingBoard : MonoBehaviour, IInteractable, IHasProgress
{
    [Header("Chopping Board Settings")]
    [SerializeField] private float choppingTime = 3f;
    private float timer = 0f;
    private bool isChopping = false;
    private bool hasFinishedChopping = false;
    private GameObject storedItem;
    private DrinkType leafType;

    [Header("Visuals")]
    [SerializeField] private GameObject choppingItemPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject choppedLeavesPrefab;


    // Chopping Board should output a chopped tea leaf of the same leafType
    // This tea leaf is a prerequisite of brewing station

    public event Action<float, bool> OnProgressChanged;

    [Header("Recipe Settings")]
    [SerializeField] private ChoppingBoardRequirements[] requirements;

    public float Progress01
    {
        get
        {
            if (isChopping)
            {
                float normalized = 1f - (timer / choppingTime);
                return Mathf.Clamp01(normalized);
            }
            else
            {
                if (hasFinishedChopping)
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
            if (isChopping)
                return true;
            else
                return false;
        }
    }

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
            bool accepted = TryAcceptingIngredient(player);

            if (accepted && AllRequirementsMet())
            {
                isChopping = true;
                timer = choppingTime;
            }
        }

        else if (hasFinishedChopping)
        {
            if (storedItem != null)
            {
                player.PickUp(storedItem);
                storedItem = null;
                hasFinishedChopping = false;
                ResetRequirements();
                Debug.Log("ChoppingBoard: Player takes chopped Tea Leaves");
            }
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

                if (storedItem != null)
                {
                    Destroy(storedItem);
                }

                storedItem = Instantiate(choppedLeavesPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                Debug.Log("Chopping Board: Tea Leaves has been Chopped!");
                RaiseProgressChanged();
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
                return false;
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
        string requiredTag = held.tag;

        foreach (var req in requirements)
        {
            // skip requirements that are already full
            if (req.currentAmount >= req.requiredAmount) continue;
            for (int i = 0; i < req.acceptableTags.Length; i++)
            {
                if (!held.CompareTag(req.acceptableTags[i]))
                    continue;
            }

            for (int i = 0;i < req.acceptableTags.Length; i++)
            {
                // Capture which tea leaf so we know which brewed tea to spawn
                if (requiredTag == req.acceptableTags[i])
                {
                    leafType = held.GetComponent<Leaf>().GetLeafType();
                    player.PlaceItem(spawnPoint);
                    StoreItemAsNextChild();
                }
                else
                {
                    player.PlaceItem(spawnPoint);
                    StoreItemAsNextChild();
                }
            }

            req.currentAmount++;
            return true;
        }

        Debug.Log("Chopping Board: This ingredient cannot be chopped.");
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

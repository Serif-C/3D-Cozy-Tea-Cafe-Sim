using System;
using UnityEngine;

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
        // Accepts Tea leaf
        if (!isChopping && !hasFinishedChopping)
        {
            if (player.HeldItemHasTag("Tea Leaf"))
            {
                leafType = player.gameObject.GetComponentInChildren<Leaf>().GetLeafType();
                player.PlaceItem(spawnPoint);

                StoreItemAsNextChild();

                Debug.Log("ChoppingBoard: Started Chopping Tea Leaves!");
                isChopping = true;
                timer = choppingTime;
            }
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

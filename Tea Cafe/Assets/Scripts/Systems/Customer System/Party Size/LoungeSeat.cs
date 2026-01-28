using UnityEngine;

public class LoungeSeat : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TransformTarget seatTarget;

    private bool isServed;
    private DrinkItem servedDrinkMeta;

    public TransformTarget GetSeatTarget()
    {
        if (seatTarget != null) return seatTarget;
        return GetComponent<TransformTarget>();
    }

    public Transform GetSpawnPoint() => spawnPoint;

    public bool HasDrinkOfType(DrinkType type)
    {
        return isServed
            && spawnPoint != null
            && spawnPoint.childCount > 0
            && servedDrinkMeta != null
            && servedDrinkMeta.DrinkType == type;
    }

    public bool CanInteract(PlayerInteractor player)
    {
        // only accept tea while not served
        return !isServed;
    }

    public void Interact(PlayerInteractor player)
    {
        if (isServed) return;
        if (player == null) return;
        if (!player.HeldItemHasTag("Tea")) return;

        player.PlaceItem(spawnPoint);

        if (spawnPoint != null && spawnPoint.childCount > 0)
        {
            var last = spawnPoint.GetChild(spawnPoint.childCount - 1);
            servedDrinkMeta = last != null ? last.GetComponent<DrinkItem>() : null;
        }

        isServed = true;
    }

    public void ClearServedItemIfAny()
    {
        isServed = false;
        servedDrinkMeta = null;
    }
}

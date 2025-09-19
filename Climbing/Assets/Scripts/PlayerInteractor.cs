using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float interactRange;   // Max interact range
    public bool isHoldingItem = false;

    [Header("Carry")]
    public Transform carryItemPostion; // Position of the item on player's hand when held
    [SerializeField] private GameObject heldItem;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Interactable");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactRange, layerMask))
        {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);

            // Check if the hit object has an IInteractable
            IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();

            if (interactableObject != null && interactableObject.CanInteract(this))
            {
                interactableObject.Interact(this);
            }
        }
    }

    public void PickUp(GameObject item)
    {
        if (isHoldingItem || item == null) return;

        isHoldingItem = true;
        heldItem = item;

        // Parent to the carry point so it follows the hand automatically
        heldItem.transform.SetParent(carryItemPostion, worldPositionStays: false);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }

    public void PlaceItem(GameObject item, Transform placementPos)
    {
        if (isHoldingItem)
        {

            // places the item on a surface and de-parent it from the player
            item.transform.SetParent(placementPos);
            item.transform.localPosition = placementPos;
            isHoldingItem = false;
        }
    }

    public bool IsHoldingItem()
    {
        return isHoldingItem;
    }

}

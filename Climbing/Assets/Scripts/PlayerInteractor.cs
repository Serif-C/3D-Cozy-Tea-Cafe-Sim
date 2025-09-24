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

    public void PlaceItem(Transform placementPos)
    {
        if (!isHoldingItem || heldItem == null || placementPos == null) return;

        heldItem.transform.SetParent(placementPos, worldPositionStays: false);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        // Optional: re-enable collider/physics so it sits properly on the surface
        if (heldItem.TryGetComponent<Collider>(out var col))
            col.enabled = true;
        if (heldItem.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true; // keep kinematic on counters; set false if you want it to fall

        // clear carry state
        heldItem = null;
        isHoldingItem = false;
    }

    public bool IsHoldingItem()
    {
        return isHoldingItem;
    }

    public string GetHeldItemType()
    {
        return heldItem.GetType().ToString();
    }


}

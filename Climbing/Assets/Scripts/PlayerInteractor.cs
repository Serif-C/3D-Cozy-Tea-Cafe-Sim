using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float interactRange;   // Max interact range
    private bool canHoldItem = true;
    public Transform carryItemPostion; // Position of the item on player's hand when held

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
        canHoldItem = false;
    }

    public bool CanHoldItem()
    {
        return canHoldItem;
    }
}

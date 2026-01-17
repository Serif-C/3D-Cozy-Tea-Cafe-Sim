using UnityEngine;

public class Cabinet : MonoBehaviour, IInteractable
{
    [Header("Cabinet Settings")]
    [SerializeField] private GameObject cabinetUI_Container;

    private Canvas cabinetCanvas;
    private bool isCanvasActive = false;

    private void Awake()
    {
        cabinetCanvas = cabinetUI_Container.gameObject.GetComponentInChildren<Canvas>();
    }

    public string Prompt
    {
        get
        {
            return "cabinet...";
        }
    }

    public bool CanInteract(PlayerInteractor player)
    {
        if (player.IsHoldingItem()) return false;

        // if there is no canvas or canvas is already active
        if (cabinetCanvas == null || isCanvasActive) return false;

        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        // open the tea cabinet canvas
        cabinetCanvas.gameObject.SetActive(true);
    }
}

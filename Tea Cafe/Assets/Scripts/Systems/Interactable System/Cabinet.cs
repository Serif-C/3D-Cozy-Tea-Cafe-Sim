using UnityEngine;

public class Cabinet : MonoBehaviour, IInteractable
{
    [Header("Cabinet Settings")]
    [SerializeField] private Canvas cabinetUI_Container;

    private Canvas cabinetCanvas;
    private bool isCanvasActive = false;

    private void Awake()
    {
        cabinetUI_Container = gameObject.GetComponentInChildren<Canvas>();
        cabinetUI_Container.enabled = false;
        cabinetCanvas = cabinetUI_Container.GetComponent<Canvas>();
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
        TimeManager.Instance.Pause();
        // open the tea cabinet canvas
        cabinetCanvas.enabled = true;
    }
}

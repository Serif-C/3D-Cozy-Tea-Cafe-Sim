using UnityEngine;

public class TeaPowder : MonoBehaviour
{
    private Canvas canvas;
    private PlayerInteractor player;
    [SerializeField] private GameObject teaPrefab;
    

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        player = FindAnyObjectByType<PlayerInteractor>().gameObject.GetComponent<PlayerInteractor>();
        teaPrefab = gameObject.GetComponent<TeaLeaf>().GetTeaPrefab();
    }

    public void OnClickTeaPowder()
    {
        // After clicking the button, pick up tea then close the canvas
        
        if (!player.IsHoldingItem())
        {
            canvas.enabled = false;
            player.PickUp(teaPrefab);
            TimeManager.Instance.Resume();
        }

    }


}

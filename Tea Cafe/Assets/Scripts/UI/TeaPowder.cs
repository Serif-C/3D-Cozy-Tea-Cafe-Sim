using UnityEngine;

public class TeaPowder : MonoBehaviour
{
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnClickTeaPowder()
    {
        // After clicking the button, pick up tea then close the canvas
        canvas.gameObject.SetActive(false);
    }


}

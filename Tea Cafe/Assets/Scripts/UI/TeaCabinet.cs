using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeaCabinet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image highlightImage;

    private void Awake()
    {
        // If you didn't assign it, fallback to Image on this object
        if (highlightImage == null)
            highlightImage = GetComponent<Image>();

        // Start hidden
        if (highlightImage != null)
            highlightImage.enabled = false;
        else
            Debug.LogError($"{name}: No Image found to toggle.");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImage != null) highlightImage.enabled = true;
        Debug.Log("Hover ENTER: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null) highlightImage.enabled = false;
        Debug.Log("Hover EXIT: " + gameObject.name);
    }
}

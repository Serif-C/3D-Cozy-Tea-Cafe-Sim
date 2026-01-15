using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeaCabinet : MonoBehaviour
{
    [SerializeField] private Sprite openCabinet;
    [SerializeField] private GameObject teaLeafPowderObject;

    private Button button;
    private Image image;

    private void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void OnClickCabinet()
    {
        image.sprite = openCabinet;
        button.targetGraphic = image;
        Debug.Log("cabinet clicked");
    }
}

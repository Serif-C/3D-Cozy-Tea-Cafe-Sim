using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeaCabinet : MonoBehaviour
{
    [SerializeField] private Sprite openCabinet;
    [SerializeField] private GameObject teaLeafPowderObject;
    [SerializeField] private bool isCabinetOpen = false;

    private Button button;
    public Image image;
    public Sprite originalCabinetSprite;


    private void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        originalCabinetSprite = gameObject.GetComponent<Image>().sprite;
    }


    /// <summary>
    /// When the cabinet opens, it switched to the openCabinet image,
    /// then the teaLeafPowderObject game object becomes active,
    /// where it can be clicked and give the player the corresponding tea powder.
    /// </summary>
    public void OnClickCabinet()
    {
        if (!isCabinetOpen)
        {
            isCabinetOpen = true;
            image.sprite = openCabinet;
            button.targetGraphic = image;
            teaLeafPowderObject.SetActive(true);
        }
        else
        {
            isCabinetOpen = false;
            image.sprite = originalCabinetSprite;
            button.targetGraphic = image;
            teaLeafPowderObject.SetActive(false);
        }
    }
}

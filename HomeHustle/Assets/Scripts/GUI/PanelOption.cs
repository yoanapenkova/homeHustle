using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PanelOption : NetworkBehaviour
{
    public GameObject element;
    public Image elementIcon;
    public InventorySlot associatedSlot;

    private bool occupied;

    void Start()
    {

    }

    void Update()
    {
        updateAppearance();
    }

    void updateAppearance()
    {
        if (element != null && !occupied)
        {
            gameObject.GetComponent<Image>().sprite = elementIcon.sprite;
            Color currentColor = gameObject.GetComponent<Image>().color;
            currentColor.a = 1;
            gameObject.GetComponent<Image>().color = currentColor;
            occupied = true;

            Debug.Log("Appearance updated!");
        }
        else if (occupied && element == null)
        {
            gameObject.GetComponent<Image>().sprite = null;
            Color currentColor = gameObject.GetComponent<Image>().color;
            currentColor.a = 0;
            gameObject.GetComponent<Image>().color = currentColor;
            elementIcon = null;
            occupied = false;
        }
    }
}

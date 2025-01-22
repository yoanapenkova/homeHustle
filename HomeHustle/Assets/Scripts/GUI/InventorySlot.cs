using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public GameObject element;
    public Image elementIcon;

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
            elementIcon = Instantiate(elementIcon);
            elementIcon.gameObject.transform.SetParent(gameObject.transform);

            RectTransform rectTransform = elementIcon.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;

            occupied = true;
        } else if (occupied && element == null)
        {
            Destroy(elementIcon.gameObject);
            elementIcon = null;
            occupied = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public GameObject element;
    public Image elementIcon;

    private bool occupied;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateAppearance();
    }

    void updateAppearance()
    {
        if (element != null && !occupied)
        {
            Debug.Log("Setting image icon");
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

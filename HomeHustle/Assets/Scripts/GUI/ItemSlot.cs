using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : NetworkBehaviour, IDropHandler
{
    [SerializeField]
    public GameObject droppedItem;
    [SerializeField]
    public GameObject initialItem;
    [SerializeField]
    public IngredientType waitingFor;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (eventData.pointerDrag != null)
        {
            droppedItem = eventData.pointerDrag;
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            eventData.pointerDrag.GetComponent<DragDrop>().inSlot = gameObject;
        }
    }

    public void ResetSlot()
    {
        initialItem.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        initialItem.GetComponent<DragDrop>().inSlot = gameObject;
        droppedItem = initialItem;
    }

}

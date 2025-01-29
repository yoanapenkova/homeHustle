using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ItemColor
{
    Red, Orange, Yellow, Green, Blue, Purple, White
}

public enum ClothType
{
    Shoes, TShirt, Shorts, Cap
}

public enum IngredientType
{
    Tomato, Cheese, Lettuce, Meat, TopBun, BottomBun
}

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    public ItemColor color;
    [SerializeField]
    public ClothType clothType;
    [SerializeField]
    public IngredientType ingredientType;

    private RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public GameObject inSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        canvasGroup.alpha = .5f;
        canvasGroup.blocksRaycasts = false;

        if (inSlot != null)
        {
            inSlot.GetComponent<ItemSlot>().droppedItem = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

    public void OnDrop(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}

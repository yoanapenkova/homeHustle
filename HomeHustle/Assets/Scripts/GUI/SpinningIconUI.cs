using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningIconUI : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 150f;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform not found. Make sure this script is attached to a UI element.");
        }
    }

    private void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}

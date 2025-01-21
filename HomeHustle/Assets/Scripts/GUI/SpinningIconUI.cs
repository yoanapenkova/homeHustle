using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningIconUI : MonoBehaviour
{
    // Speed of the rotation in degrees per second
    [SerializeField]
    private float rotationSpeed = 150f;

    // The RectTransform of the UI element to spin
    private RectTransform rectTransform;

    private void Start()
    {
        // Get the RectTransform component attached to this GameObject
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform not found. Make sure this script is attached to a UI element.");
        }
    }

    private void Update()
    {
        // Rotate the UI element around the Z-axis
        if (rectTransform != null)
        {
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}

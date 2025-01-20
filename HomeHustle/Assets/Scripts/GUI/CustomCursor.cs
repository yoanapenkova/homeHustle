using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField]
    private Texture2D cursorTexture; // Assign the texture in the inspector

    [SerializeField]
    private Vector2 hotspot = new Vector2(0, 0); // Hotspot defines the "clickable" point of the cursor

    void Start()
    {
        // Change cursor appearance when the game starts
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        //Cursor.visible = true; // Ensure the cursor is visible
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

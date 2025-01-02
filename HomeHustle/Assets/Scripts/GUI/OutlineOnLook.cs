using System.Collections.Generic;
using UnityEngine;

public class OutlineOnLook : MonoBehaviour
{
    [SerializeField] private Camera playerCamera; // Reference to the player's camera
    [SerializeField] private float maxDistance = 10f; // Max distance for the raycast
    [SerializeField] private Material outlineMaterial; // Outline material
    [SerializeField] private LayerMask interactableLayer;
    private GameObject currentObject; // Currently highlighted object

    // A dictionary to store original materials for multiple child renderers
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    [Header("UI Management")]
    [SerializeField]
    private GameObject actionsInstructions;

    void Update()
    {
        // Perform a Raycast
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // If the hit object is new, apply the outline
            if (hitObject != currentObject)
            {
                ClearOutline(); // Remove outline from previous object
                ApplyOutline(hitObject);
            }

            //Debug.Log(hitObject.gameObject.name);

            // Check object is on watch for UI hints
            Interactable interactableProperties = hitObject.GetComponent<Interactable>();
            if (interactableProperties != null)
            {
                interactableProperties.isOnWatch = true;
            }
        }
        else
        {
            // No object hit, clear any existing outline
            ClearOutline();
        }
    }

    void ApplyOutline(GameObject obj)
    {
        currentObject = obj;

        // Find all Renderers on the object and its children
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && !originalMaterials.ContainsKey(renderer))
            {
                // Save the original materials array
                originalMaterials[renderer] = renderer.materials;

                // Create a new materials array with the same length
                Material[] outlineMaterials = new Material[renderer.materials.Length];

                // Replace each material with the outline material
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    outlineMaterials[i] = outlineMaterial;
                }

                // Apply the new materials array
                renderer.materials = outlineMaterials;
            }
        }
    }

    void ClearOutline()
    {
        if (currentObject == null) return;

        // Restore original materials for all renderers
        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.materials = entry.Value; // Restore the original materials array
            }
        }

        // Uncheck object is on watch for UI hints
        Interactable interactableProperties = currentObject.GetComponent<Interactable>();
        if (interactableProperties != null)
        {
            interactableProperties.isOnWatch = false;
        }

        // Clear the state
        originalMaterials.Clear();
        currentObject = null;

        // Clear the UI
        actionsInstructions.SetActive(false);
    }
}

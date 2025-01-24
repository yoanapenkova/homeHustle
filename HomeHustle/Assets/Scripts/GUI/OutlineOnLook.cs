using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class OutlineOnLook : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private LayerMask interactableLayer;
    private GameObject currentObject;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    [Header("UI Management")]
    [SerializeField]
    private GameObject actionsInstructions;

    private PlayerManager playerManager;

    void Update()
    {
        if(!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc();
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (playerManager != null)
        {
            if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
            {
                GameObject hitObject = hit.collider.gameObject;

                Interactable interactableProperties = hitObject.GetComponent<Interactable>();
                if (interactableProperties.playerRoles.Contains(playerManager.role))
                {
                    if (interactableProperties != null)
                    {
                        interactableProperties.isOnWatch = true;
                    }

                    if (hitObject != currentObject)
                    {
                        ClearOutline();
                        ApplyOutline(hitObject);
                    }
                }
            }
            else
            {
                ClearOutline();
            }
        }

        if (currentObject != null)
        {
            if (!currentObject.GetComponent<Interactable>().enabled)
            {
                ClearOutline();
            }
        }
    }

    void ApplyOutline(GameObject obj)
    {
        currentObject = obj;

        Interactable interactableProperties = currentObject.GetComponent<Interactable>();

        if (interactableProperties.enabled)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && !originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.materials;
                    Material[] outlineMaterials = new Material[renderer.materials.Length];

                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        outlineMaterials[i] = outlineMaterial;
                    }

                    renderer.materials = outlineMaterials;
                }
            }
        }
    }

    void ClearOutline()
    {
        if (currentObject == null) return;

        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.materials = entry.Value;
            }
        }

        Interactable interactableProperties = currentObject.GetComponent<Interactable>();
        if (interactableProperties != null)
        {
            interactableProperties.isOnWatch = false;
        }

        originalMaterials.Clear();
        currentObject = null;
        actionsInstructions.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckForNetworkAndPlayerServerRpc()
    {
        if (NetworkManager.Singleton.LocalClientId != null)
        {
            // Get the player object for the specified client ID
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var client))
            {
                playerManager = client.PlayerObject.gameObject.GetComponent<PlayerManager>();
            }
            else
            {
                Debug.LogError($"Client ID {NetworkManager.Singleton.LocalClientId} not found!");
            }
        }
    }
}

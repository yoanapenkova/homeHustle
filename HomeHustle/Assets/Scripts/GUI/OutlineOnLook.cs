using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField]
    public GameObject mainKey;
    [SerializeField]
    public Sprite mainKeyOriginalSprite;
    [SerializeField]
    public GameObject washUI;
    [SerializeField]
    public TMP_Text mainNeededPoints;
    [SerializeField]
    public TMP_Text auxNeededPoints;

    private PlayerManager playerManager;

    void Update()
    {
        if(!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (playerManager != null && GameManager.Instance.gameStarted)
        {
            if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
            {
                GameObject hitObject = hit.collider.gameObject;

                Interactable interactableProperties = hitObject.GetComponent<Interactable>();
                if (interactableProperties.playerRoles.Contains(playerManager.role))
                {
                    interactableProperties.isOnWatch = true;

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
                mainKey.GetComponent<Image>().sprite = mainKeyOriginalSprite;
                mainNeededPoints.text = "";
                auxNeededPoints.text = "";
            }
        }

        if (currentObject != null)
        {
            if (!currentObject.GetComponent<Interactable>().enabled)
            {
                ClearOutline();
                mainKey.GetComponent<Image>().sprite = mainKeyOriginalSprite;
                mainNeededPoints.text = "";
                auxNeededPoints.text = "";
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
        washUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckForNetworkAndPlayerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject playerObject = client.PlayerObject.gameObject;

            if (playerObject != null)
            {
                AssignPlayerManagerClientRpc(playerObject.GetComponent<NetworkObject>().NetworkObjectId, clientId);
            }
            else
            {
                Debug.LogError($"PlayerManager not found on Client ID {clientId}");
            }
        }
    }

    [ClientRpc]
    void AssignPlayerManagerClientRpc(ulong playerObjectId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectId].gameObject;
            playerManager = playerObject.GetComponent<PlayerManager>();
        }
    }
}

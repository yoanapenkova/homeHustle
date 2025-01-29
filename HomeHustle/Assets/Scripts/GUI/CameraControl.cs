using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CameraControl : NetworkBehaviour
{
    [Header("UX/UI Management")]
    [SerializeField]
    private GameObject[] panelsOverHUD;

    private PlayerManager playerManager;
    private GameObject activePanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        ControlCamera();
        ControlPanel();
    }

    void ControlCamera()
    {
        foreach (GameObject panel in panelsOverHUD)
        {
            if (panel.activeSelf)
            {
                playerManager.cameraMovement = false;
                activePanel = panel;
                break;
            }
        }
    }

    void ControlPanel()
    {
        if (activePanel != null)
        {
            if (!activePanel.activeSelf)
            {
                playerManager.cameraMovement = true;
                activePanel = null;
            }
        }
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

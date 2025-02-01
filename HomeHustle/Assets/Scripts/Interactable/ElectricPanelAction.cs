using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ElectricPanelAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Connect" };

    [Header("UI Management")]
    [SerializeField]
    private GameObject electricPanelManagementPanel;
    [SerializeField]
    private TMP_Text costText;
    [SerializeField]
    private Button cancelButton;

    private Interactable interactable;
    private bool panelOpened = false;

    private PlayerManager playerManager;

    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (interactable.isOnWatch && !panelOpened)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        electricPanelManagementPanel.SetActive(true);
        costText.text = playerManager.isHuman ? "each action will cost -2" : "each action will cost -5";

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => HideEPManagementPanel());
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.mainInstructionsText.text = actions[0];

        interactable.auxKeyBackground.SetActive(false);
        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;
    }

    void HideEPManagementPanel()
    {
        electricPanelManagementPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

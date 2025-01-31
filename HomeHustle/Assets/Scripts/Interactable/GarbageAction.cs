using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GarbageAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private InventorySlot[] playerSlots;
    [SerializeField]
    private TMP_Text actionText;

    private string[] actions = { "Throw away inventory items" };
    private Interactable interactable;
    private GameObject[] elementsInInventory = new GameObject[4];

    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            Outcome();
        } else
        {
            actionText.gameObject.SetActive(false);
        }
        
    }

    void SaveActualItems()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i].element != null)
            {
                elementsInInventory[i] = playerSlots[i].element;
            }
            else
            {
                elementsInInventory[i] = null;
            }

        }
    }

    void DestroyItem()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i; // Map Alpha1, Alpha2, etc., to slot indices
            if (Input.GetKeyDown(key))
            {
                if (elementsInInventory[i] != null)
                {
                    if (elementsInInventory[i].GetComponent<Interactable>().throwable)
                    {
                        GameObject objToDestroy = elementsInInventory[i].gameObject;
                        elementsInInventory[i] = null;
                        Destroy(objToDestroy);
                    } else
                    {
                        playerManager.gameObject.GetComponent<InventoryManagement>().missNextThrow = true;
                        string message = "Item cannot be thrown away!";
                        UIManager.Instance.ShowFeedback(message);
                    }
                    
                } else
                {
                    string message = "Inventory slot is empty!";
                    UIManager.Instance.ShowFeedback(message);
                }
            }
        }
    }

    public void Outcome()
    {
        SaveActualItems();
        DestroyItem();
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(false);
        interactable.auxKeyBackground.SetActive(false);
        actionText.text = actions[0];
        actionText.gameObject.SetActive(true);
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

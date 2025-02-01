using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RetrieveAction : NetworkBehaviour
{
    [SerializeField]
    private GameObject[] inventorySlots;

    private InventorySlot inventorySlot;

    void Start()
    {
        inventorySlot = gameObject.GetComponent<InventorySlot>();
    }

    void Update()
    {
        
    }

    public void RetrieveItem()
    {
        InventorySlot freeSlot = getFirstFreeSlot();

        if (freeSlot != null)
        {
            freeSlot.element = inventorySlot.element;
            freeSlot.elementIcon = inventorySlot.element.GetComponent<PickUpAction>().imagePrefab;
            retrieveObjectLocalServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            string message = "You have no free slots!";
            UIManager.Instance.ShowFeedback(message);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void retrieveObjectLocalServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var player))
        {
            GameObject playerObject = player.PlayerObject.gameObject;

            inventorySlot.element.transform.SetParent(playerObject.transform);
            inventorySlot.element.transform.localPosition = new Vector3(0, 1, 0.5f); // Adjust if needed

            removeObjectFromContainerInventoryClientRpc();
        }
        else
        {
            Debug.LogError($"Client ID {clientId} not found!");
        }
    }

    [ClientRpc(RequireOwnership = false)]
    void removeObjectFromContainerInventoryClientRpc()
    {
        /*
        if (!IsServer)
        {
            Debug.Log("client");
        }
        Debug.Log("ENTRA QUITAR ELEMENT");
        InventorySlot inventorySlotObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[inventorySlotId].gameObject.GetComponent<InventorySlot>();
        */
        inventorySlot.element = null;
    }

    private InventorySlot getFirstFreeSlot()
    {
        InventorySlot res = null;

        foreach (GameObject slot in inventorySlots)
        {
            InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
            if (inventorySlot.element == null)
            {
                res = inventorySlot;
                break;
            }
        }

        return res;
    }
}

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
            Debug.Log("Antes: " + NetworkManager.Singleton.LocalClientId);
            retrieveObjectLocalServerRpc(NetworkManager.Singleton.LocalClientId);

            freeSlot.element = inventorySlot.element;
            freeSlot.elementIcon = inventorySlot.element.GetComponent<PickUpAction>().imagePrefab;
            inventorySlot.element = null;
        }
        else
        {
            // TODO: Display message saying inventory is full.
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void retrieveObjectLocalServerRpc(ulong clientId)
    {
        Debug.Log("Después: " + clientId);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var player))
        {
            GameObject playerObject = player.PlayerObject.gameObject;

            inventorySlot.element.transform.SetParent(playerObject.transform);
            inventorySlot.element.transform.localPosition = new Vector3(0, 1, 0.5f); // Adjust if needed
        }
        else
        {
            Debug.LogError($"Client ID {clientId} not found!");
        }
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

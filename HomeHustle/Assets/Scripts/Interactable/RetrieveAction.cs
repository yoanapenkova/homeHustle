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

    // Start is called before the first frame update
    void Start()
    {
        inventorySlot = gameObject.GetComponent<InventorySlot>();
    }

    // Update is called once per frame
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

        // Get the player object for the specified object ID
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var player))
        {
            //Debug.Log(container.name);
            GameObject playerObject = player.PlayerObject.gameObject;

            // Parent the object to the player's "pocket" or an empty GameObject on the player
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

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StoreAction : NetworkBehaviour
{
    public ContainerInventory containerInventory;
    public GameObject[] containerInventorySlots;

    private InventorySlot inventorySlot;

    void Start()
    {
        inventorySlot = gameObject.GetComponent<InventorySlot>();
    }

    void Update()
    {
        
    }

    public void StoreItem()
    {
        ulong containerId = containerInventory.currentObjectInventory.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        ulong inventorySlotItemId = inventorySlot.element.GetComponent<NetworkObject>().NetworkObjectId;
        bool isServerRequesting = IsServer;

        InventorySlot freeSlot = getFirstFreeSlot();

        if (freeSlot != null)
        {
            Debug.Log("BEFORE CONTAINER ID " + containerId);
            Debug.Log("BEFORE ITEM ID " + inventorySlotItemId);
            StoreObjectLocalServerRpc(containerId, inventorySlotItemId);
            AddItemToContainerInventoryServerRpc(freeSlot.GetComponent<NetworkObject>().NetworkObjectId, inventorySlotItemId, NetworkManager.Singleton.LocalClientId, isServerRequesting);
            inventorySlot.element = null;
        }
        else
        {
            string message = "Container inventory is full!";
            UIManager.Instance.ShowFeedback(message);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    void StoreObjectLocalServerRpc(ulong objectId, ulong itemId)
    {
        Debug.Log("AFTER " + objectId);
        Debug.Log("AFTER " + itemId);
        
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var container))
        {
            GameObject containerObject = container.gameObject;
            
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
            {
                GameObject itemObject = item.gameObject;
                itemObject.transform.SetParent(containerObject.transform);
                itemObject.transform.localPosition = new Vector3(0, 1, 0.5f); // Adjust if needed
            }
            else
            {
                Debug.LogError($"Object ID {itemId} not found!");
            }
        }
        else
        {
            Debug.LogError($"Object ID {objectId} not found!");
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    void AddItemToContainerInventoryServerRpc(ulong containerSlotId, ulong itemId, ulong clientId, bool isServer)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(containerSlotId, out var containerSlot))
        {
            InventorySlot containerSlotObject = containerSlot.gameObject.GetComponent<InventorySlot>();

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
            {
                GameObject itemObject = item.gameObject;
                containerSlotObject.element = itemObject;
                containerSlotObject.elementIcon = itemObject.GetComponent<PickUpAction>().imagePrefab;

                addItemOnClientSideClientRpc(containerSlotId, itemId, clientId, isServer);
            }
            else
            {
                Debug.LogError($"Object ID {itemId} not found!");
            }
        }
        else
        {
            Debug.LogError($"Object ID {containerSlotId} not found!");
        }
    }

    [ClientRpc]
    void addItemOnClientSideClientRpc(ulong containerSlotId, ulong itemId, ulong clientId, bool isServer)
    {
        if (isServer || ((clientId == NetworkManager.Singleton.LocalClientId) && !isServer))
        {
            InventorySlot containerSlotObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[containerSlotId].gameObject.GetComponent<InventorySlot>();
            GameObject itemObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId].gameObject;

            containerSlotObject.element = itemObject;
            containerSlotObject.elementIcon = itemObject.GetComponent<PickUpAction>().imagePrefab;
        }
    }

    private InventorySlot getFirstFreeSlot()
    {
        InventorySlot res = null;

        foreach (GameObject slot in containerInventorySlots)
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

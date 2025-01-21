using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StoreAction : NetworkBehaviour
{
    [SerializeField]
    private ContainerInventory containerInventory;
    [SerializeField]
    private GameObject[] containerInventorySlots;

    private InventorySlot inventorySlot;
    private ulong containerId;
    

    // Start is called before the first frame update
    void Start()
    {
        inventorySlot = gameObject.GetComponent<InventorySlot>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StoreItem()
    {
        containerId = containerInventory.currentObjectInventory.gameObject.GetComponent<NetworkObject>().NetworkObjectId;

        InventorySlot freeSlot = getFirstFreeSlot();

        if (freeSlot != null)
        {
            Debug.Log("Antes de pasar: " + containerId);
            storeObjectLocalServerRpc(containerId);

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
    void storeObjectLocalServerRpc(ulong objectId)
    {
        Debug.Log("Después de pasar: " + objectId);
        
        // Get the player object for the specified object ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var container))
        {
            //Debug.Log(container.name);
            GameObject containerObject = container.gameObject;
            
            // Parent the object to the player's "pocket" or an empty GameObject on the player
            inventorySlot.element.transform.SetParent(containerObject.transform);
            inventorySlot.element.transform.localPosition = new Vector3(0, 1, 0.5f); // Adjust if needed
        }
        else
        {
            Debug.LogError($"Object ID {objectId} not found!");
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

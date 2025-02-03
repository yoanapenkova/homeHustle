using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class InventoryManagement : NetworkBehaviour
{
    [SerializeField]
    public GameObject[] inventorySlots;

    [SerializeField]
    public Transform shootingPoint;

    [SerializeField]
    public float shootForce = 10f;

    private GameObject objectToParent;
    private InventorySlot currentSlotThrowing;

    public bool isEnabled = true;
    public bool missNextThrow = false;
    public bool canThrow = true;

    void Awake()
    {
        GameObject[] unsortedSlots = GameObject.FindGameObjectsWithTag("InventorySlot");

        inventorySlots = unsortedSlots
            .Where(obj => !obj.name.Contains("Container"))
            .OrderBy(obj => obj.name)
            .ToArray();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (!IsOwner) return;

        ShootElements();
    }
    
    void StartShootElement(ulong clientId, NetworkObjectReference elementReference, GameObject placeholder)
    {
        if (elementReference.TryGet(out NetworkObject elementNetworkObject))
        {
            GameObject element = elementNetworkObject.gameObject;

            if (element != null)
            {
                element.GetComponent<PickUpAction>().ChangePickUpState();

                ulong objectId = element.GetComponent<NetworkObject>().NetworkObjectId;
                DeparentElementServerRpc(objectId);

                if (currentSlotThrowing.isDirected)
                {
                    ChangePositionServerRpc(element.GetComponent<NetworkObject>().NetworkObjectId, placeholder.GetComponent<NetworkObject>().NetworkObjectId);
                    
                    currentSlotThrowing.isDirected = false;
                    currentSlotThrowing.directedTransform = null;
                    currentSlotThrowing = null;
                    isEnabled = true;
                }
                else
                {
                    ShootServerRpc(clientId, objectId);
                }
            }
        }
        else
        {
            Debug.LogWarning("Failed to get element from NetworkObjectReference.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangePositionServerRpc(ulong elementId, ulong placeholderId)
    {
        GameObject element = NetworkManager.Singleton.SpawnManager.SpawnedObjects[elementId].gameObject;
        GameObject placeholder = NetworkManager.Singleton.SpawnManager.SpawnedObjects[placeholderId].gameObject;
        if (element != null && placeholder != null)
        {
            element.transform.position = placeholder.transform.position;
            element.transform.rotation = placeholder.transform.rotation;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DeparentElementServerRpc(ulong objectId)
    {
        GameObject elementObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;
        elementObject.transform.SetParent(null);
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootServerRpc(ulong clientId, ulong objectId)
    {
        Transform shootingPoint = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.Find("ShootingInventoryElement");
        GameObject elementObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;
        Rigidbody rb = elementObject.GetComponent<Rigidbody>();
        if (shootingPoint != null)
        {
            Vector3 shootDirection = shootingPoint.forward;
            rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("ShootingPoint not found for client.");
        }
    }


    void ShootElements()
    {
        if (!isEnabled) return; // Prevents execution if disabled

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i; // Map Alpha1, Alpha2, etc., to slot indices
            if (Input.GetKeyDown(key))
            {
                Debug.Log("InventoryManagement");
                HandleSlotShoot(i, false, null);
            }
        }
    }

    public void HandleSlotShoot(int slotIndex, bool isDirected, GameObject placeholder)
    {
        if (!IsOwner) return;

        if (!canThrow) return;

        if (missNextThrow) {
            missNextThrow = false; 
            return; 
        } else {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return; // Safety check

            InventorySlot slot = inventorySlots[slotIndex].GetComponent<InventorySlot>();
            if (slot != null && slot.element != null)
            {
                GameObject element = slot.element;
                NetworkObject networkObject = element.GetComponent<NetworkObject>();
                Debug.Log(networkObject.gameObject.name);

                if (networkObject != null)
                {
                    Debug.Log("assigning current slot throwing");
                    currentSlotThrowing = slot;
                    currentSlotThrowing.isDirected = isDirected;
                    if (placeholder != null)
                    {
                        currentSlotThrowing.directedTransform = placeholder.transform;
                    }
                    StartShootElement(NetworkManager.Singleton.LocalClientId, networkObject, placeholder);
                    slot.element = null;

                    if (slot.slotTime.text != "")
                    {
                        slot.StopShootCountdown();
                        slot.slotTime.text = "";
                    }

                    // Start cooldown after throwing
                    StartCoroutine(ThrowCooldown());
                }
            }
        }
    }

    // Cooldown Coroutine
    private IEnumerator ThrowCooldown()
    {
        canThrow = false; // Disable throwing
        yield return new WaitForSeconds(1f); // Wait for 1 second
        canThrow = true; // Re-enable throwing
    }

}

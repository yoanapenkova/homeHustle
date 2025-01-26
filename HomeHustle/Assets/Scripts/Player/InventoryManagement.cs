using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;

public class InventoryManagement : NetworkBehaviour
{
    [SerializeField]
    public GameObject[] inventorySlots;

    [SerializeField]
    public Transform shootingPoint;

    [SerializeField]
    public float shootForce = 10f;

    private GameObject objectToParent;

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
        ShootElements();
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootElementServerRpc(ulong clientId, NetworkObjectReference elementReference)
    {
        if (elementReference.TryGet(out NetworkObject elementNetworkObject))
        {
            GameObject element = elementNetworkObject.gameObject;
            Rigidbody rb = element.GetComponent<Rigidbody>();

            if (rb != null)
            {
                element.GetComponent<PickUpAction>().ChangePickUpState();

                element.transform.SetParent(null);

                Transform shootingPoint = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.Find("ShootingInventoryElement");
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
        }
        else
        {
            Debug.LogWarning("Failed to get element from NetworkObjectReference.");
        }
    }


    void ShootElements()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i; // Map Alpha1, Alpha2, etc., to slot indices
            if (Input.GetKeyDown(key))
            {
                HandleSlotShoot(i);
            }
        }
    }

    void HandleSlotShoot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return; // Safety check

        InventorySlot slot = inventorySlots[slotIndex].GetComponent<InventorySlot>();
        if (slot != null && slot.element != null)
        {
            GameObject element = slot.element;
            NetworkObject networkObject = element.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                ShootElementServerRpc(NetworkManager.Singleton.LocalClientId, networkObject);
                slot.element = null;
            }
        }
    }

}

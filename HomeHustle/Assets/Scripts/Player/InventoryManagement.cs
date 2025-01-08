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

        // Sort by name and assign
        inventorySlots = unsortedSlots
            .OrderBy(obj => obj.name) // Sort alphabetically by name
            .ToArray();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
                // Change the pick-up state on the server
                element.GetComponent<PickUpAction>().ChangePickUpState();

                // Un-parent the object
                element.transform.SetParent(null);

                // Apply force to shoot the object
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InventorySlot slot = inventorySlots[0].GetComponent<InventorySlot>();
            if (slot != null && slot.element != null)
            {
                GameObject element = slot.element;
                NetworkObject networkObject = element.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    // Send a ServerRpc to perform the shooting logic on the server
                    ShootElementServerRpc(NetworkManager.Singleton.LocalClientId, networkObject);
                    slot.element = null; // Clear the inventory slot locally
                }
            }
        }
    }

}

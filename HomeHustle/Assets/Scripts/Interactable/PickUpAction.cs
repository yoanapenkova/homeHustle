using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PickUpAction : NetworkBehaviour, SimpleAction
{
    public bool pickedUp = false;
    [SerializeField]
    public Image imagePrefab;
    [SerializeField]
    private GameObject[] inventorySlots;

    private string[] actions = { "Pick up" };
    private Interactable interactable;
    private Renderer[] renderers;
    private Collider collider;
    private Rigidbody rb;

    // Networked variable to track if the tap is runnning
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        renderers = GetComponentsInChildren<Renderer>();
        collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        // Subscribe to the NetworkVariable's value change event
        isPickedUp.OnValueChanged += OnPickUpStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger door actions
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the door is activated or deactivated
        if (IsServer)
        {
            // When the server enables the object, initialize the door's state
            isPickedUp.Value = pickedUp;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isPickedUp.OnValueChanged -= OnPickUpStateChanged;
    }

    public void Outcome()
    {
        InventorySlot freeSlot = getFirstFreeSlot();

        if (freeSlot != null)
        {
            // Call the ServerRpc and pass the local player's client ID
            pickUpObjectLocalServerRpc(NetworkManager.Singleton.LocalClientId);

            freeSlot.element = gameObject;
            freeSlot.elementIcon = imagePrefab;

            ChangePickUpState();
        } else
        {
            // TODO: Display message saying inventory is full.
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.mainInstructionsText.text = actions[0];

        DecayAction decayAction = gameObject.GetComponent<DecayAction>();
        if (decayAction == null)
        {
            interactable.auxKeyBackground.SetActive(false);
        }

        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;
    }

    public void ChangePickUpState()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            TogglePickUpState();
        }
        // If we are a client, request the server to toggle the pick up state
        else
        {
            TogglePickUpStateServerRpc();
        }
    }

    // Handles the logic to toggle the pick up's state
    private void TogglePickUpState()
    {
        // Toggle the pick up's opening state
        isPickedUp.Value = !isPickedUp.Value;
        pickedUp = isPickedUp.Value;

        // Notify all clients that the pick up state has changed
        PickUpStateChangedClientRpc(isPickedUp.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the pick up state
    [ServerRpc(RequireOwnership = false)]
    private void TogglePickUpStateServerRpc()
    {
        TogglePickUpState();
    }

    // This method is called when the network variable 'isPickedUp' changes
    private void OnPickUpStateChanged(bool previousValue, bool newValue)
    {
        pickedUp = newValue;

        // Loop through all child renderers and disable/enable them
        foreach (Renderer rend in renderers)
        {
            rend.enabled = !newValue;
        }
        collider.enabled = !newValue;
        rb.isKinematic = newValue;
    }

    // ClientRpc: Used to notify clients of the pick up state change
    [ClientRpc]
    private void PickUpStateChangedClientRpc(bool newState)
    {
        pickedUp = newState;

        // Loop through all child renderers and disable/enable them
        foreach (Renderer rend in renderers)
        {
            rend.enabled = !newState;
        }
        collider.enabled = !newState;
        rb.isKinematic = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    void pickUpObjectLocalServerRpc(ulong clientId)
    {
        // Get the player object for the specified client ID
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject clientPlayerObject = client.PlayerObject.gameObject;

            // Parent the object to the player's "pocket" or an empty GameObject on the player
            gameObject.transform.SetParent(clientPlayerObject.transform);
            gameObject.transform.localPosition = new Vector3(0, 1, 0.5f); // Adjust if needed
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

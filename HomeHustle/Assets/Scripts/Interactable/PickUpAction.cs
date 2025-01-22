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
    [Header("UI Management")]
    [SerializeField]
    public Image imagePrefab;
    [SerializeField]
    private GameObject[] inventorySlots;

    private string[] actions = { "Pick up" };
    public bool pickedUp = false;
    private Interactable interactable;
    private Renderer[] renderers;
    private Collider collider;
    private Rigidbody rb;

    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);

    void Start()
    {
        interactable = GetComponent<Interactable>();
        renderers = GetComponentsInChildren<Renderer>();
        collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        isPickedUp.OnValueChanged += OnPickUpStateChanged;
    }

    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isPickedUp.Value = pickedUp;
        }
    }

    private void OnDisable()
    {
        isPickedUp.OnValueChanged -= OnPickUpStateChanged;
    }

    public void Outcome()
    {
        InventorySlot freeSlot = getFirstFreeSlot();

        if (freeSlot != null)
        {
            pickUpObjectLocalServerRpc(NetworkManager.Singleton.LocalClientId);

            freeSlot.element = gameObject;
            freeSlot.elementIcon = imagePrefab;

            ChangePickUpState();
            UIManager.Instance.ShowFeedback("Inventory is full!");
        } else
        {
            UIManager.Instance.ShowFeedback("Inventory is full!");
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
        if (IsServer)
        {
            TogglePickUpState();
        }
        else
        {
            TogglePickUpStateServerRpc();
        }
    }

    private void TogglePickUpState()
    {
        isPickedUp.Value = !isPickedUp.Value;
        pickedUp = isPickedUp.Value;

        PickUpStateChangedClientRpc(isPickedUp.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePickUpStateServerRpc()
    {
        TogglePickUpState();
    }

    private void OnPickUpStateChanged(bool previousValue, bool newValue)
    {
        pickedUp = newValue;

        foreach (Renderer rend in renderers)
        {
            rend.enabled = !newValue;
        }
        collider.enabled = !newValue;
        rb.isKinematic = newValue;
    }

    [ClientRpc]
    private void PickUpStateChangedClientRpc(bool newState)
    {
        pickedUp = newState;

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
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject clientPlayerObject = client.PlayerObject.gameObject;

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

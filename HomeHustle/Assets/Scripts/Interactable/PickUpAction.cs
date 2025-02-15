using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum Status
{
    Clean, Dirty
}

public class PickUpAction : NetworkBehaviour, SimpleAction
{
    [Header("UI Management")]
    [SerializeField]
    public Image imagePrefab;
    [SerializeField]
    public GameObject[] inventorySlots;

    private PlayerManager playerManager;

    [Header("Status")]
    [SerializeField]
    public Status status;

    private string[] actions = { "Pick up" };
    public bool pickedUp = false;
    private Interactable interactable;
    public Renderer[] renderers;
    public Collider[] colliders;
    public Rigidbody rb;

    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);

    void Start()
    {
        interactable = GetComponent<Interactable>();
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();

        isPickedUp.OnValueChanged += OnPickUpStateChanged;
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

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

            if (!playerManager.isHuman)
            {
                int index = System.Array.IndexOf(inventorySlots, freeSlot.gameObject);
                freeSlot.StartShootCountdown(index);
            }
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
        PushAction pushAction = gameObject.GetComponent<PushAction>();
        if (decayAction == null && pushAction == null)
        {
            interactable.auxKeyBackground.SetActive(false);
        }
        else if (pushAction != null)
        {
            interactable.auxKeyBackground.SetActive(true);
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
        foreach (Collider coll in colliders)
        {
            coll.enabled = !newValue;
        }
        rb.isKinematic = newValue;

        if (newValue)
        {
            WashAction washAction = gameObject.GetComponent<WashAction>();
            if (washAction != null && washAction.enabled)
            {
                washAction.insideSink = false;
                washAction.washUI.SetActive(false);
            }
        }
    }

    [ClientRpc]
    private void PickUpStateChangedClientRpc(bool newState)
    {
        pickedUp = newState;

        foreach (Renderer rend in renderers)
        {
            rend.enabled = !newState;
        }
        foreach (Collider coll in colliders)
        {
            coll.enabled = !newState;
        }
        rb.isKinematic = newState;

        if (newState)
        {
            WashAction washAction = gameObject.GetComponent<WashAction>();
            if (washAction != null)
            {
                if (washAction.enabled)
                {
                    washAction.insideSink = false;
                    washAction.washUI.SetActive(false);
                    washAction.interactionProgress = 0f;
                    washAction.progressSlider.value = washAction.interactionProgress;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void pickUpObjectLocalServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject clientPlayerObject = client.PlayerObject.gameObject;

            gameObject.transform.SetParent(clientPlayerObject.transform);
            gameObject.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // Adjust if needed
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

    [ServerRpc(RequireOwnership = false)]
    void CheckForNetworkAndPlayerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject playerObject = client.PlayerObject.gameObject;

            if (playerObject != null)
            {
                AssignPlayerManagerClientRpc(playerObject.GetComponent<NetworkObject>().NetworkObjectId, clientId);
            }
            else
            {
                Debug.LogError($"PlayerManager not found on Client ID {clientId}");
            }
        }
    }

    [ClientRpc]
    void AssignPlayerManagerClientRpc(ulong playerObjectId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectId].gameObject;
            playerManager = playerObject.GetComponent<PlayerManager>();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BedAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private GameObject madeObject;
    [SerializeField]
    private GameObject unmadeObject;

    private string[] actions = { "Make", "Unmake" };
    private bool made = false;
    private Interactable interactable;

    // Networked variable to track if the bed is made
    private NetworkVariable<bool> isMade = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        // Subscribe to the NetworkVariable's value change event
        isMade.OnValueChanged += OnBedStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger bed actions
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the bed is made or unmade
        if (IsServer)
        {
            // When the server enables the object, initialize the bed's state
            isMade.Value = made;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isMade.OnValueChanged -= OnBedStateChanged;
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleBedState();
        }
        // If we are a client, request the server to toggle the bed state
        else
        {
            ToggleBedStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (made)
        {
            interactable.mainInstructionsText.text = actions[1];
        }
        else
        {
            interactable.mainInstructionsText.text = actions[0];
        }
        interactable.auxKeyBackground.SetActive(false);
        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;
    }

    // Handles the logic to toggle the bed's state (made/unmade)
    private void ToggleBedState()
    {
        // Toggle the bed's opening state
        isMade.Value = !isMade.Value;
        made = isMade.Value;

        // Notify all clients that the bed state has changed
        BedStateChangedClientRpc(isMade.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the bed state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleBedStateServerRpc()
    {
        ToggleBedState();
    }

    // This method is called when the network variable 'isMade' changes
    private void OnBedStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            madeObject.SetActive(true);
            unmadeObject.SetActive(false);
        }
        else
        {
            madeObject.SetActive(false);
            unmadeObject.SetActive(true);
        }
    }

    // ClientRpc: Used to notify clients of the bed state change
    [ClientRpc]
    private void BedStateChangedClientRpc(bool newState)
    {
        if (newState)
        {
            madeObject.SetActive(true);
            unmadeObject.SetActive(false);
        }
        else
        {
            madeObject.SetActive(false);
            unmadeObject.SetActive(true);
        }
    }
}

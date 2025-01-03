using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class LightSwitchAction : NetworkBehaviour, SimpleAction
{

    [SerializeField]
    private Light lightObject;

    private string[] actions = { "Turn on", "Turn off" };
    private bool turnedOn = false;
    private Animator animator;
    private Interactable interactable;

    // Networked variable to track if the light is turned on
    private NetworkVariable<bool> isTurned = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();

        // Subscribe to the NetworkVariable's value change event
        isTurned.OnValueChanged += OnLightStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger light actions
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the light is activated or deactivated
        if (IsServer)
        {
            // When the server enables the object, initialize the light's state
            isTurned.Value = turnedOn;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isTurned.OnValueChanged -= OnLightStateChanged;
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleLightState();
        }
        // If we are a client, request the server to toggle the light state
        else
        {
            ToggleLightStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        if (turnedOn)
        {
            interactable.mainInstructionsText.text = actions[1];
        }
        else
        {
            interactable.mainInstructionsText.text = actions[0];
        }
    }

    // Handles the logic to toggle the light's state (open/close)
    private void ToggleLightState()
    {
        // Toggle the light's opening state
        isTurned.Value = !isTurned.Value;
        turnedOn = isTurned.Value;

        // Notify all clients that the light state has changed
        LightStateChangedClientRpc(isTurned.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the light state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleLightStateServerRpc()
    {
        ToggleLightState();
    }

    // This method is called when the network variable 'isTurned' changes
    private void OnLightStateChanged(bool previousValue, bool newValue)
    {
        // Update the animator when the light's state changes
        animator.SetBool("isTurned", newValue);
        lightObject.gameObject.SetActive(newValue);
    }

    // ClientRpc: Used to notify clients of the light state change
    [ClientRpc]
    private void LightStateChangedClientRpc(bool newState)
    {
        // Only the client who receives the RPC will update its own animator
        animator.SetBool("isTurned", newState);
        lightObject.gameObject.SetActive(newState);
    }
}

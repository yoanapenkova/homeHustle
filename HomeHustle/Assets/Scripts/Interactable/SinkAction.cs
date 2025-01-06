using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class SinkAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private ParticleSystem particleEffects;

    private string[] actions = { "Open", "Close" };
    private bool open = false;
    private Interactable interactable;

    // Networked variable to track if the tap is runnning
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        // Subscribe to the NetworkVariable's value change event
        isRunning.OnValueChanged += OnSinkStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger sink actions
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
            isRunning.Value = open;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isRunning.OnValueChanged -= OnSinkStateChanged;
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleSinkState();
        }
        // If we are a client, request the server to toggle the door state
        else
        {
            ToggleSinkStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (open)
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

    // Handles the logic to toggle the sink's state (open/close)
    private void ToggleSinkState()
    {
        // Toggle the sink's opening state
        isRunning.Value = !isRunning.Value;
        open = isRunning.Value;

        // Notify all clients that the sink state has changed
        SinkStateChangedClientRpc(isRunning.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the sink state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleSinkStateServerRpc()
    {
        ToggleSinkState();
    }

    // This method is called when the network variable 'isRunning' changes
    private void OnSinkStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            EnableWaterFlow();
        }
        else
        {
            DisableWaterFlow();
        }
    }

    // ClientRpc: Used to notify clients of the door state change
    [ClientRpc]
    private void SinkStateChangedClientRpc(bool newState)
    {
        if (newState)
        {
            EnableWaterFlow();
        }
        else
        {
            DisableWaterFlow();
        }

    }

    private void EnableWaterFlow()
    {
        if (particleEffects != null)
        {
            particleEffects.gameObject.SetActive(true);
            particleEffects.Play();
        }
    }

    private void DisableWaterFlow()
    {
        if (particleEffects != null)
        {
            particleEffects.Stop();
            particleEffects.gameObject.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WaterComponentAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private Button boilerPanelButton;
    [SerializeField]
    private GameObject warningSign;
    [SerializeField]
    private GameObject warningSignUI;

    private string[] actions = { "Fix" };
    public bool broken = false;
    private Interactable interactable;
    private SinkAction sinkAction;

    // Networked variable to track if the water component is broken
    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = gameObject.GetComponent<Interactable>();
        sinkAction = gameObject.GetComponent<SinkAction>();

        isBroken.OnValueChanged += OnBrokenStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();

            if (Input.GetKeyUp(KeyCode.Q) && broken)
            {
                Outcome();
            }
        }
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the water component is broken
        if (IsServer)
        {
            // When the server enables the object, initialize the component's broken state
            isBroken.Value = broken;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isBroken.OnValueChanged -= OnBrokenStateChanged;
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleBrokenState();
        }
        // If we are a client, request the server to toggle the broken state
        else
        {
            ToggleBrokenStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);
        interactable.auxInstructionsText.text = actions[0];

        if (broken)
        {
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
        else
        {
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
            interactable.mainKey.GetComponent<Image>().color = Color.white;
            interactable.mainInstructionsText.color = Color.white;
        }

        if (sinkAction == null)
        {
            interactable.mainKeyBackground.SetActive(false);
        }
    }

    // Handles the logic to toggle the component's broken state
    private void ToggleBrokenState()
    {
        // Toggle the component's broken state
        isBroken.Value = !isBroken.Value;
        broken = isBroken.Value;

        boilerPanelButton.interactable = !broken;

        // Notify all clients that the broken state has changed
        BrokenStateChangedClientRpc(isBroken.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the broken state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleBrokenStateServerRpc()
    {
        ToggleBrokenState();
    }

    // This method is called when the network variable 'isBroken' changes
    private void OnBrokenStateChanged(bool previousValue, bool newValue)
    {
        warningSign.SetActive(newValue);
        warningSignUI.SetActive(newValue);
    }

    // ClientRpc: Used to notify clients of the broken state change
    [ClientRpc]
    private void BrokenStateChangedClientRpc(bool newState)
    {
        warningSign.SetActive(newState);
        warningSignUI.SetActive(newState);
    }
}

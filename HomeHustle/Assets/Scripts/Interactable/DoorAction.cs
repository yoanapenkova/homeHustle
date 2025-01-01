using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class DoorAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = {"Open", "Close"};
    private bool opened = false;
    private Animator animator;
    private Interactable interactable;

    // Networked variable to track if the door is open
    private NetworkVariable<bool> isOpening = new NetworkVariable<bool>(false);

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleDoorState();
        }
        // If we are a client, request the server to toggle the door state
        else
        {
            ToggleDoorStateServerRpc();
        }
    }

    // ServerRpc: Used by clients to request the server to toggle the door state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorStateServerRpc()
    {
        ToggleDoorState();
    }

    // Handles the logic to toggle the door's state (open/close)
    private void ToggleDoorState()
    {
        // Toggle the door's opening state
        isOpening.Value = !isOpening.Value;
        opened = isOpening.Value;

        // Notify all clients that the door state has changed
        DoorStateChangedClientRpc(isOpening.Value);
    }

    // This method is called when the network variable 'isOpening' changes
    private void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        // Update the animator when the door's state changes
        animator.SetBool("isOpening", newValue);
    }

    // ClientRpc: Used to notify clients of the door state change
    [ClientRpc]
    private void DoorStateChangedClientRpc(bool newState)
    {
        // Only the client who receives the RPC will update its own animator
        animator.SetBool("isOpening", newState);

    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();

        // Subscribe to the NetworkVariable's value change event
        isOpening.OnValueChanged += OnDoorStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        // Allow any client to trigger door actions
        if (Input.GetKeyDown(KeyCode.E) && interactable.isOnWatch)
        {
            Outcome();
        }

        UpdateInstructions();
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the door is activated or deactivated
        if (IsServer)
        {
            // When the server enables the object, initialize the door's state
            isOpening.Value = opened;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isOpening.OnValueChanged -= OnDoorStateChanged;
    }

    void UpdateInstructions()
    {
        if (interactable.isOnWatch)
        {
            interactable.actionsInstructions.SetActive(true);
            if (opened)
            {
                interactable.mainInstructionsText.text = actions[1];
            } else
            {
                interactable.mainInstructionsText.text = actions[0];
            }
        } else
        {
            interactable.actionsInstructions.SetActive(false);
            interactable.mainInstructionsText.text = "";
        }
    }
}

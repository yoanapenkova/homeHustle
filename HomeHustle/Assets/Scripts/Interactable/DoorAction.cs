using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class DoorAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = {"Open", "Close"};
    public bool opened = false;
    private Animator animator;
    private Interactable interactable;
    private LockAction lockAction;

    // Networked variable to track if the door is open
    private NetworkVariable<bool> isOpening = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();
        lockAction = GetComponent<LockAction>();

        // Subscribe to the NetworkVariable's value change event
        isOpening.OnValueChanged += OnDoorStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger door actions
            if (Input.GetKeyDown(KeyCode.E) && !lockAction.locked)
            {
                Outcome();
            } else if (lockAction.locked)
            {
                //TODO: Incluir ruido de prohibido al intentar abrir una puerta bloqueada.
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
            isOpening.Value = opened;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isOpening.OnValueChanged -= OnDoorStateChanged;
    }

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

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (opened)
        {
            interactable.mainInstructionsText.text = actions[1];
        }
        else
        {
            interactable.mainInstructionsText.text = actions[0];
        }
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

    // ServerRpc: Used by clients to request the server to toggle the door state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorStateServerRpc()
    {
        ToggleDoorState();
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

    
}

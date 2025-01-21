using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PowerAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private Button electricPanelButton;

    public bool multipleObjects = false;
    public bool powered = true;
    private Interactable interactable;

    // Networked variable to track if the water component is broken
    private NetworkVariable<bool> isPowered = new NetworkVariable<bool>(true);

    // Start is called before the first frame update
    void Start()
    {
        interactable = gameObject.GetComponent<Interactable>();

        isPowered.OnValueChanged += OnPowerStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the object is powered
        if (IsServer)
        {
            // When the server enables the object, initialize the object's powered state
            isPowered.Value = powered;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isPowered.OnValueChanged -= OnPowerStateChanged;
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            TogglePowerState();
        }
        // If we are a client, request the server to toggle the powered state
        else
        {
            TogglePowerStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        throw new System.NotImplementedException();
    }

    // Handles the logic to toggle the component's powered state
    private void TogglePowerState()
    {
        if (multipleObjects)
        {
            GameObject[] powerActionObjects = GameObject.FindGameObjectsWithTag(gameObject.tag);

            foreach (GameObject gameObj in powerActionObjects)
            {
                PowerAction powerAction = gameObj.GetComponent<PowerAction>();
                // Toggle the component's powered state
                powerAction.isPowered.Value = !powerAction.isPowered.Value;
                powerAction.powered = powerAction.isPowered.Value;
            }
        } else
        {
            // Toggle the component's powered state
            isPowered.Value = !isPowered.Value;
            powered = isPowered.Value;
        }

        // Notify all clients that the powered state has changed
        PowerStateChangedClientRpc(isPowered.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the powered state
    [ServerRpc(RequireOwnership = false)]
    private void TogglePowerStateServerRpc()
    {
        TogglePowerState();
    }

    // This method is called when the network variable 'isPowered' changes
    private void OnPowerStateChanged(bool previousValue, bool newValue)
    {
        if (multipleObjects)
        {
            GameObject[] powerActionObjects = GameObject.FindGameObjectsWithTag(gameObject.tag);

            foreach (GameObject gameObj in powerActionObjects)
            {
                PowerAction powerAction = gameObj.GetComponent<PowerAction>();
                powerAction.powered = newValue;
            }
        }
        else
        {
            powered = newValue;
        }

        if (newValue)
        {
            electricPanelButton.image.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        } else
        {
            electricPanelButton.image.transform.localScale = new Vector3(0.6f, -0.6f, 0.6f);
        }
        
    }

    // ClientRpc: Used to notify clients of the powered state change
    [ClientRpc]
    private void PowerStateChangedClientRpc(bool newState)
    {
        if (multipleObjects)
        {
            GameObject[] powerActionObjects = GameObject.FindGameObjectsWithTag(gameObject.tag);

            foreach (GameObject gameObj in powerActionObjects)
            {
                PowerAction powerAction = gameObj.GetComponent<PowerAction>();
                powerAction.powered = newState;
            }
        }
        else
        {
            powered = newState;
        }

        if (newState)
        {
            electricPanelButton.image.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        else
        {
            electricPanelButton.image.transform.localScale = new Vector3(0.6f, -0.6f, 0.6f);
        }
    }
}

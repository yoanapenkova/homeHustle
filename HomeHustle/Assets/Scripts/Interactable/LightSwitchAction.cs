using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LightSwitchAction : NetworkBehaviour, SimpleAction
{

    [SerializeField]
    private Light lightObject;
    [SerializeField]
    private GameObject lampObject;
    [SerializeField]
    private Material emissionMaterial;

    private string[] actions = { "Turn on", "Turn off" };
    private bool turnedOn = false;
    private Animator animator;
    private Interactable interactable;
    private Renderer lampRenderer;
    private Material glassMaterial;

    // Networked variable to track if the light is turned on
    private NetworkVariable<bool> isTurned = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();
        lampRenderer = lampObject.GetComponent<Renderer>();
        if (lampRenderer != null )
        {
            glassMaterial = lampRenderer.materials[0];
        }

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
        interactable.mainKeyBackground.SetActive(true);
        if (turnedOn)
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
        if (newValue)
        {
            Material[] materials = lampRenderer.materials;
            materials[0] = emissionMaterial;
            lampRenderer.materials = materials;
        } else
        {
            Material[] materials = lampRenderer.materials;
            materials[0] = glassMaterial;
            lampRenderer.materials = materials;
        }
    }

    // ClientRpc: Used to notify clients of the light state change
    [ClientRpc]
    private void LightStateChangedClientRpc(bool newState)
    {
        // Only the client who receives the RPC will update its own animator
        animator.SetBool("isTurned", newState);
        lightObject.gameObject.SetActive(newState);
        if (newState)
        {
            Material[] materials = lampRenderer.materials;
            materials[0] = emissionMaterial;
            lampRenderer.materials = materials;
        }
        else
        {
            Material[] materials = lampRenderer.materials;
            materials[0] = glassMaterial;
            lampRenderer.materials = materials;
        }
    }
}

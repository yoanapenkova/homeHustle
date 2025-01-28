using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class LightSwitchAction : NetworkBehaviour, SimpleAction
{
    [Header("Cost Management")]
    [SerializeField]
    private int costPerObject = 3;

    private PlayerManager playerManager;

    [SerializeField]
    private Light lightObject;
    [SerializeField]
    private GameObject lampObject;
    [SerializeField]
    private Material emissionMaterial;

    private string[] actions = { "Turn on", "Turn off" };
    public bool turnedOn = false;
    private Animator animator;
    private Interactable interactable;
    private PowerAction powerAction;
    private Renderer lampRenderer;
    private Material glassMaterial;

    private NetworkVariable<bool> isTurned = new NetworkVariable<bool>(true);

    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();
        powerAction = GetComponent<PowerAction>();
        lampRenderer = lampObject.GetComponent<Renderer>();
        if (lampRenderer != null )
        {
            glassMaterial = lampRenderer.materials[0];
        }

        isTurned.OnValueChanged += OnLightStateChanged;
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

        if (!powerAction.powered)
        {
            ChangeLightAppearance(false);
        } else
        {
            ChangeLightAppearance(turnedOn);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        OnLightStateChanged(false, isTurned.Value);
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isTurned.Value = turnedOn;
        }
    }

    private void OnDisable()
    {
        isTurned.OnValueChanged -= OnLightStateChanged;
    }

    public void Outcome()
    {
        bool isAllowed = playerManager.isHuman || (!playerManager.isHuman && ((playerManager.points - costPerObject) >= 0));
        if (isAllowed)
        {
            if (IsServer)
            {
                ToggleLightState();
            }
            else
            {
                ToggleLightStateServerRpc();
            }
        } else
        {
            string message = "Need more energy!";
            UIManager.Instance.ShowFeedback(message);
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

    private void ToggleLightState()
    {
        isTurned.Value = !isTurned.Value;
        turnedOn = isTurned.Value;

        LightStateChangedClientRpc(isTurned.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleLightStateServerRpc()
    {
        ToggleLightState();
    }

    private void OnLightStateChanged(bool previousValue, bool newValue)
    {
        animator.SetBool("isTurned", newValue);
        turnedOn = newValue;
    }

    [ClientRpc]
    private void LightStateChangedClientRpc(bool newState)
    {
        animator.SetBool("isTurned", newState);
        turnedOn = newState;
        
    }

    void ChangeLightAppearance(bool isOn)
    {
        lightObject.gameObject.SetActive(isOn);
        if (isOn)
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

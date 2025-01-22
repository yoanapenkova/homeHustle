using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PowerAction : NetworkBehaviour, SimpleAction
{
    [Header("UI Management")]
    [SerializeField]
    private Button electricPanelButton;

    public bool multipleObjects = false;
    public bool powered = true;
    private Interactable interactable;

    private NetworkVariable<bool> isPowered = new NetworkVariable<bool>(true);

    void Start()
    {
        interactable = gameObject.GetComponent<Interactable>();

        isPowered.OnValueChanged += OnPowerStateChanged;
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isPowered.Value = powered;
        }
    }

    private void OnDisable()
    {
        isPowered.OnValueChanged -= OnPowerStateChanged;
    }

    public void Outcome()
    {
        if (IsServer)
        {
            TogglePowerState();
        }
        else
        {
            TogglePowerStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        throw new System.NotImplementedException();
    }

    private void TogglePowerState()
    {
        if (multipleObjects)
        {
            GameObject[] powerActionObjects = GameObject.FindGameObjectsWithTag(gameObject.tag);

            foreach (GameObject gameObj in powerActionObjects)
            {
                PowerAction powerAction = gameObj.GetComponent<PowerAction>();
                powerAction.isPowered.Value = !powerAction.isPowered.Value;
                powerAction.powered = powerAction.isPowered.Value;
            }
        } else
        {
            isPowered.Value = !isPowered.Value;
            powered = isPowered.Value;
        }

        PowerStateChangedClientRpc(isPowered.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePowerStateServerRpc()
    {
        TogglePowerState();
    }

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

        electricPanelButton.image.transform.localScale = newState ? new Vector3(0.6f, 0.6f, 0.6f) : new Vector3(0.6f, -0.6f, 0.6f);
    }
}

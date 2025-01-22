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
    public bool open = false;
    private Interactable interactable;
    private WaterComponentAction waterComponentAction;

    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false);

    void Start()
    {
        interactable = GetComponent<Interactable>();
        waterComponentAction = GetComponent<WaterComponentAction>();

        isRunning.OnValueChanged += OnSinkStateChanged;
    }

    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E) && !waterComponentAction.broken)
            {
                Outcome();
            }
        }
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isRunning.Value = open;
        }
    }

    private void OnDisable()
    {
        isRunning.OnValueChanged -= OnSinkStateChanged;
    }

    public void Outcome()
    {
        if (IsServer)
        {
            ToggleSinkState();
        }
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
    }

    private void ToggleSinkState()
    {
        isRunning.Value = !isRunning.Value;
        open = isRunning.Value;

        SinkStateChangedClientRpc(isRunning.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleSinkStateServerRpc()
    {
        ToggleSinkState();
    }

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

using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BedAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private GameObject madeObject;
    [SerializeField]
    private GameObject unmadeObject;

    private string[] actions = { "Make", "Unmake" };
    private bool made = false;
    private Interactable interactable;

    private NetworkVariable<bool> isMade = new NetworkVariable<bool>(false);

    void Start()
    {
        interactable = GetComponent<Interactable>();

        isMade.OnValueChanged += OnBedStateChanged;
    }

    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isMade.Value = made;
        }
    }

    private void OnDisable()
    {
        isMade.OnValueChanged -= OnBedStateChanged;
    }

    public void Outcome()
    {
        if (IsServer)
        {
            ToggleBedState();
        }
        else
        {
            ToggleBedStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (made)
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

    private void ToggleBedState()
    {
        isMade.Value = !isMade.Value;
        made = isMade.Value;

        BedStateChangedClientRpc(isMade.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleBedStateServerRpc()
    {
        ToggleBedState();
    }

    private void OnBedStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            madeObject.SetActive(true);
            unmadeObject.SetActive(false);
        }
        else
        {
            madeObject.SetActive(false);
            unmadeObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void BedStateChangedClientRpc(bool newState)
    {
        if (newState)
        {
            madeObject.SetActive(true);
            unmadeObject.SetActive(false);
        }
        else
        {
            madeObject.SetActive(false);
            unmadeObject.SetActive(true);
        }
    }
}

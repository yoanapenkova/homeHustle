using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FreshUpAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private GameObject gamePanel;

    private string[] actions = { "Fresh Up" };
    public bool dadDone = false;
    public bool momDone = false;
    public bool boyDone = false;
    public bool girlDone = false;

    private Interactable interactable;

    private NetworkVariable<bool> isDadDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMomDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBoyDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isGirlDone = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        isDadDone.OnValueChanged += OnDadStateChanged;
        isMomDone.OnValueChanged += OnMomStateChanged;
        isBoyDone.OnValueChanged += OnBoyStateChanged;
        isGirlDone.OnValueChanged += OnGirlStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();

            if (Input.GetKeyUp(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        gamePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.mainInstructionsText.text = actions[0];

        interactable.auxKeyBackground.SetActive(false);
        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;
    }

    public void UpdateDad()
    {
        if (IsServer)
        {
            ToggleDadState();
        } else
        {
            ToggleDadStateServerRpc();
        }
    }

    private void ToggleDadState()
    {
        isDadDone.Value = !isDadDone.Value;
        dadDone = isDadDone.Value;

        DadStateChangedClientRpc(isDadDone.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDadStateServerRpc()
    {
        ToggleDadState();
    }

    private void OnDadStateChanged(bool previousValue, bool newValue)
    {
        dadDone = newValue;
    }

    [ClientRpc]
    private void DadStateChangedClientRpc(bool newState)
    {
        dadDone = newState;
    }

    public void UpdateMom()
    {
        if (IsServer)
        {
            ToggleMomState();
        }
        else
        {
            ToggleMomStateServerRpc();
        }
    }

    private void ToggleMomState()
    {
        isMomDone.Value = !isMomDone.Value;
        momDone = isMomDone.Value;

        MomStateChangedClientRpc(isMomDone.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleMomStateServerRpc()
    {
        ToggleMomState();
    }

    private void OnMomStateChanged(bool previousValue, bool newValue)
    {
        momDone = newValue;
    }

    [ClientRpc]
    private void MomStateChangedClientRpc(bool newState)
    {
        momDone = newState;
    }

    public void UpdateBoy()
    {
        if (IsServer)
        {
            ToggleBoyState();
        }
        else
        {
            ToggleBoyStateServerRpc();
        }
    }

    private void ToggleBoyState()
    {
        isBoyDone.Value = !isBoyDone.Value;
        boyDone = isBoyDone.Value;

        BoyStateChangedClientRpc(isBoyDone.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleBoyStateServerRpc()
    {
        ToggleBoyState();
    }

    private void OnBoyStateChanged(bool previousValue, bool newValue)
    {
        boyDone = newValue;
    }

    [ClientRpc]
    private void BoyStateChangedClientRpc(bool newState)
    {
        boyDone = newState;
    }

    public void UpdateGirl()
    {
        if (IsServer)
        {
            ToggleGirlState();
        }
        else
        {
            ToggleGirlStateServerRpc();
        }
    }

    private void ToggleGirlState()
    {
        isGirlDone.Value = !isGirlDone.Value;
        girlDone = isGirlDone.Value;

        GirlStateChangedClientRpc(isGirlDone.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleGirlStateServerRpc()
    {
        ToggleGirlState();
    }

    private void OnGirlStateChanged(bool previousValue, bool newValue)
    {
        girlDone = newValue;
    }

    [ClientRpc]
    private void GirlStateChangedClientRpc(bool newState)
    {
        girlDone = newState;
    }
}

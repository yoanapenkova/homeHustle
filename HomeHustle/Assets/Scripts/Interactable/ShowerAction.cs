using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShowerAction : NetworkBehaviour, SimpleAction
{
    [Header("UI Management")]
    [SerializeField]
    public GameObject actionsInstructions;
    [SerializeField]
    public GameObject mainKeyBackground;
    [SerializeField]
    public GameObject mainKey;
    [SerializeField]
    public TMP_Text mainInstructionsText;
    [SerializeField]
    public GameObject auxKeyBackground;
    [SerializeField]
    private GameObject showerUI;
    [SerializeField]
    private SinkAction showerObject;
    [SerializeField]
    private Slider progressSlider;
    [SerializeField]
    private float interactionSpeed = 1f;
    [SerializeField]
    private float targetProgress = 20f;
    private float interactionProgress = 0f;

    private string[] actions = { "Fresh Up" };
    public bool dadDone = false;
    public bool momDone = false;
    public bool boyDone = false;
    public bool girlDone = false;

    public bool insideShower = false;
    private bool actionCompleted = false;

    private NetworkVariable<bool> isDadDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMomDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBoyDone = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isGirlDone = new NetworkVariable<bool>(false);

    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        isDadDone.OnValueChanged += OnDadStateChanged;
        isMomDone.OnValueChanged += OnMomStateChanged;
        isBoyDone.OnValueChanged += OnBoyStateChanged;
        isGirlDone.OnValueChanged += OnGirlStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (insideShower && !actionCompleted)
        {
            UpdateInstructions();

            if (Input.GetKey(KeyCode.E) && showerObject.open)
            {
                Outcome();
            }
        } else
        {
            showerUI.SetActive(false);
            interactionProgress = 0f;
            progressSlider.value = interactionProgress;
        }
    }

    public void Outcome()
    {
        interactionProgress += interactionSpeed * Time.deltaTime;
        interactionProgress = Mathf.Clamp(interactionProgress, 0f, targetProgress);
        progressSlider.value = interactionProgress;

        if (interactionProgress >= targetProgress && !actionCompleted)
        {
            if (playerManager.role == PlayerRole.Dad && !dadDone)
            {
                UpdateDad();
            }
            else if (playerManager.role == PlayerRole.Mom && !momDone)
            {
                UpdateMom();
            }
            else if (playerManager.role == PlayerRole.Boy && !boyDone)
            {
                UpdateBoy();
            }
            else if (playerManager.role == PlayerRole.Girl && !girlDone)
            {
                UpdateGirl();
            }
            actionCompleted = true;
        }
    }

    public void UpdateInstructions()
    {
        actionsInstructions.SetActive(true);
        mainKeyBackground.SetActive(true);
        auxKeyBackground.SetActive(false);
        if (showerObject.open)
        {
            mainInstructionsText.text = "Hold to shower";
            mainKey.GetComponent<Image>().color = Color.white;
            mainInstructionsText.color = Color.white;
        } else
        {
            mainInstructionsText.text = "Turn on the shower";
            mainKey.GetComponent<Image>().color = Color.grey;
            mainInstructionsText.color = Color.grey;
        }
        showerUI.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.GetComponent<PlayerManager>().role == playerManager.role)
        {
            insideShower = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.GetComponent<PlayerManager>().role == playerManager.role)
        {
            insideShower = false;
        }
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

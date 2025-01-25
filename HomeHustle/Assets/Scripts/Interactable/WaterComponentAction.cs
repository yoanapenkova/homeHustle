using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaterComponentAction : NetworkBehaviour, SimpleAction
{
    [Header("Cost Management")]
    [SerializeField]
    private int costPerHuman = 8;
    [SerializeField]
    private int costPerObject = 10;

    private PlayerManager playerManager;

    [Header("UI Management")]
    [SerializeField]
    private Button boilerPanelButton;
    [SerializeField]
    private GameObject warningSign;
    [SerializeField]
    private GameObject warningSignUI;

    private string[] actions = { "Fix" };
    public bool broken = false;
    private Interactable interactable;
    private SinkAction sinkAction;
    private ContainerAction containerAction;

    // Cooldown logic
    private bool isBeingRepaired = false;
    private int timesBroken;
    [Header("FX Management")]
    [SerializeField]
    private ParticleSystem smokeFX;
    [SerializeField]
    private GameObject cooldownSignUI;

    // Networked variable to track if the water component is broken
    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = gameObject.GetComponent<Interactable>();
        sinkAction = gameObject.GetComponent<SinkAction>();
        containerAction = gameObject.GetComponent<ContainerAction>();

        isBroken.OnValueChanged += OnBrokenStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (interactable.isOnWatch)
        {
            UpdateInstructions();

            if (Input.GetKeyUp(KeyCode.Q) && broken)
            {
                Outcome();
            }
        }

        if (playerManager == null)
        {
            CheckForNetworkAndPlayer();
        }
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the water component is broken
        if (IsServer)
        {
            // When the server enables the object, initialize the component's broken state
            isBroken.Value = broken;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isBroken.OnValueChanged -= OnBrokenStateChanged;
    }

    public void Outcome()
    {
        bool isAllowed = playerManager.isHuman ? ((playerManager.points - costPerHuman) >= 0) : ((playerManager.points - costPerObject) >= 0);

        if (isAllowed)
        {
            timesBroken++;
            // If we are the server, we handle the state change
            if (IsServer)
            {
                ToggleBrokenState();
            }
            // If we are a client, request the server to toggle the broken state
            else
            {
                ToggleBrokenStateServerRpc();
            }

            if (playerManager.isHuman)
            {
                playerManager.points -= costPerHuman;
            }
            else
            {
                playerManager.points += costPerObject;
            }
        }
        else
        {
            string message = playerManager.isHuman ? "Need more coins!" : "Need more energy!";
            UIManager.Instance.ShowFeedback(message);
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);
        interactable.auxInstructionsText.text = actions[0];

        if (broken || isBeingRepaired)
        {
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
        else
        {
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
            if (containerAction == null)
            {
                interactable.mainKey.GetComponent<Image>().color = Color.white;
                interactable.mainInstructionsText.color = Color.white;
            }
        }

        if (sinkAction == null && containerAction == null)
        {
            interactable.mainKeyBackground.SetActive(false);
        }
        else if (sinkAction == null && containerAction != null)
        {
            interactable.mainKeyBackground.SetActive(true);
        }
    }

    // Handles the logic to toggle the component's broken state
    private void ToggleBrokenState()
    {
        // Toggle the component's broken state
        isBroken.Value = !isBroken.Value;
        broken = isBroken.Value;

        // Notify all clients that the broken state has changed
        BrokenStateChangedClientRpc(isBroken.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the broken state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleBrokenStateServerRpc()
    {
        ToggleBrokenState();
    }

    // This method is called when the network variable 'isBroken' changes
    private void OnBrokenStateChanged(bool previousValue, bool newValue)
    {
        broken = newValue;
        warningSign.SetActive(newValue);
        warningSignUI.SetActive(newValue);

        if (!broken)
        {
            interactable.enabled = false;
            smokeFX.gameObject.SetActive(true);
            cooldownSignUI.SetActive(true);
            isBeingRepaired = true;
            StartCoroutine(Repair());
            StartCoroutine(ActivateIcon());
        } else
        {
            boilerPanelButton.interactable = !broken;
        }
    }

    // ClientRpc: Used to notify clients of the broken state change
    [ClientRpc]
    private void BrokenStateChangedClientRpc(bool newState)
    {
        broken = newState;
        warningSign.SetActive(newState);
        warningSignUI.SetActive(newState);

        if (!broken)
        {
            interactable.enabled = false;
            smokeFX.gameObject.SetActive(true);
            cooldownSignUI.SetActive(true);
            isBeingRepaired = true;
            StartCoroutine(Repair());
            StartCoroutine(ActivateIcon());
        } else
        {
            boilerPanelButton.interactable = !broken;
        }
    }

    private IEnumerator Repair()
    {
        yield return new WaitForSeconds(5);
        AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.bellSound);
        smokeFX.gameObject.SetActive(false);
        isBeingRepaired = false;
        interactable.enabled = true;
    }

    private IEnumerator ActivateIcon()
    {

        if (timesBroken == 1)
        {
            yield return new WaitForSeconds(30);
        } else if (timesBroken == 2)
        {
            yield return new WaitForSeconds(45);
        } else if (timesBroken > 2)
        {
            yield return new WaitForSeconds(60);
        }
        cooldownSignUI.SetActive(false);
        boilerPanelButton.interactable = !broken;
    }

    void CheckForNetworkAndPlayer()
    {
        if (NetworkManager.Singleton.LocalClientId != null)
        {
            SearchPlayerManagerServerRpc(OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SearchPlayerManagerServerRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Get the player object for the specified client ID
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                playerManager = client.PlayerObject.gameObject.GetComponent<PlayerManager>();
            }
            else
            {
                Debug.LogError($"Client ID {clientId} not found!");
            }
        }
    }
}

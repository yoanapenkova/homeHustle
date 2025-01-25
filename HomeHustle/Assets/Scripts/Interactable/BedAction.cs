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
    public bool made = false;
    private Interactable interactable;

    private bool isBeingMade = false;
    [Header("FX Management")]
    [SerializeField]
    private ParticleSystem smokeFX;

    [Header("UI Management")]
    [SerializeField]
    private GameObject minigamePanel;

    private NetworkVariable<bool> isMade = new NetworkVariable<bool>(false);

    private PlayerManager playerManager;

    void Start()
    {
        interactable = GetComponent<Interactable>();

        isMade.OnValueChanged += OnBedStateChanged;
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (playerManager != null)
        {
            if (interactable.isOnWatch)
            {
                UpdateInstructions();

                if ((Input.GetKeyDown(KeyCode.E) && !made && playerManager.isHuman) || (Input.GetKeyDown(KeyCode.E) && made && !playerManager.isHuman))
                {
                    Outcome();
                }
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
        if (playerManager.isHuman)
        {
            PerformStateChange();
        } else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            minigamePanel.SetActive(true);
            minigamePanel.GetComponent<ClickerMinigame>().RefreshGame();
            minigamePanel.GetComponent<ClickerMinigame>().bed = this;
        }
    }

    public void PerformStateChange()
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
        if (playerManager != null)
        {
            interactable.actionsInstructions.SetActive(true);
            interactable.mainKeyBackground.SetActive(true);

            if (playerManager.isHuman)
            {
                interactable.mainInstructionsText.text = actions[0];
                if (!made)
                {
                    interactable.mainKey.GetComponent<Image>().color = Color.white;
                    interactable.mainInstructionsText.color = Color.white;
                } else if (made || isBeingMade)
                {
                    interactable.mainKey.GetComponent<Image>().color = Color.grey;
                    interactable.mainInstructionsText.color = Color.grey;
                }
            }
            else
            {
                interactable.mainInstructionsText.text = actions[1];
                if (made)
                {
                    interactable.mainKey.GetComponent<Image>().color = Color.white;
                    interactable.mainInstructionsText.color = Color.white;
                }
                else
                {
                    interactable.mainKey.GetComponent<Image>().color = Color.grey;
                    interactable.mainInstructionsText.color = Color.grey;
                }
            }

            interactable.auxKeyBackground.SetActive(false);
        }
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
        made = newValue;
        unmadeObject.SetActive(!newValue);

        if (made)
        {
            interactable.enabled = false;
            smokeFX.gameObject.SetActive(true);
            isBeingMade = true;
            StartCoroutine(Make());
        } else
        {
            madeObject.SetActive(false);
        }
    }

    private IEnumerator Make()
    {
        yield return new WaitForSeconds(5);
        AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.bellSound);
        smokeFX.gameObject.SetActive(false);
        isBeingMade = false;
        interactable.enabled = true;
        madeObject.SetActive(true);
    }

    [ClientRpc]
    private void BedStateChangedClientRpc(bool newState)
    {
        made = newState;
        unmadeObject.SetActive(!newState);

        if (made)
        {
            interactable.enabled = false;
            smokeFX.gameObject.SetActive(true);
            isBeingMade = true;
            StartCoroutine(Make());
        } else
        {
            madeObject.SetActive(false);
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

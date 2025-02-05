using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ProductCondition
{
    Perfect, Normal, Bad
}

public class DecayAction : NetworkBehaviour, SimpleAction
{
    [Header("Cost Management")]
    [SerializeField]
    private int costPerObject = 10;

    private PlayerManager playerManager;

    [Header("UI Management")]
    [SerializeField]
    private TMP_Text timeLeftText;
    [SerializeField]
    private TMP_Text conditionText;
    [SerializeField]
    private int timeToSpoil = 120;

    private string[] actions = { "Spoil" };
    public bool altered;
    private ProductCondition condition;
    private Interactable interactable;
    private Camera mainCamera;

    private NetworkVariable<bool> isAltered = new NetworkVariable<bool>(false);

    void Start()
    {
        interactable = GetComponent<Interactable>();
        mainCamera = Camera.main;

        isAltered.OnValueChanged += OnAlteredStateChanged;
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
            
            if (Input.GetKeyDown(KeyCode.Q) && !altered && !playerManager.isHuman)
            {
                Outcome();
            }
            timeLeftText.gameObject.SetActive(true);
            conditionText.gameObject.SetActive(true);
        } else
        {
            timeLeftText.gameObject.SetActive(false);
            conditionText.gameObject.SetActive(false);
        }
        UpdateGUI();
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isAltered.Value = altered;
        }
    }

    private void OnDisable()
    {
        isAltered.OnValueChanged -= OnAlteredStateChanged;
    }

    void UpdateCondition()
    {
        if (timeToSpoil > 75)
        {
            condition = ProductCondition.Perfect;
            conditionText.text = "Perfect";
            conditionText.color = Color.green;
        } else if (timeToSpoil > 15)
        {
            condition = ProductCondition.Normal;
            conditionText.text = "Normal";
            conditionText.color = Color.yellow;
        } else
        {
            condition = ProductCondition.Bad;
            conditionText.text = "Bad";
            conditionText.color = Color.red;
        }

    }

    void UpdateGUI()
    {
        timeLeftText.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        conditionText.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void Outcome()
    {
        bool isAllowed = (playerManager.points - costPerObject) >= 0;
        if (isAllowed)
        {
            if (IsServer)
            {
                ToggleAlteredState();
            }
            else
            {
                ToggleAlteredStateServerRpc();
            }
            if (!playerManager.isHuman)
            {
                playerManager.points -= costPerObject;
                GameStats.Instance.spentPoints += costPerObject;
                if (!GameStats.Instance.tamperedItemsState.Value)
                {
                    GameStats.Instance.UpdateTamperSabotageServerRpc(true);
                }
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
        interactable.auxKeyBackground.SetActive(!playerManager.isHuman);
        interactable.auxInstructionsText.text = actions[0];

        if (altered)
        {
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
        } else
        {
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
            interactable.auxNeededPoints.text = "cost: -" + costPerObject;
        }
    }

    private void ToggleAlteredState()
    {
        isAltered.Value = !isAltered.Value;
        altered = isAltered.Value;

        AlteredStateChangedClientRpc(isAltered.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleAlteredStateServerRpc()
    {
        ToggleAlteredState();
    }

    private void OnAlteredStateChanged(bool previousValue, bool newValue)
    {
        altered = newValue;
    }

    [ClientRpc]
    private void AlteredStateChangedClientRpc(bool newState)
    {
        altered = newState;
        StartCoroutine(ShowTime());
    }

    private IEnumerator ShowTime()
    {
        while (timeToSpoil > 0)
        {
            UpdateCondition();
            int minutes = Mathf.FloorToInt(timeToSpoil / 60); // Calculate the minutes
            int seconds = Mathf.FloorToInt(timeToSpoil % 60); // Calculate the seconds
            timeLeftText.text = $"{minutes:00}:{seconds:00}"; // Format as MM:SS

            yield return new WaitForSeconds(1);
            timeToSpoil--;
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

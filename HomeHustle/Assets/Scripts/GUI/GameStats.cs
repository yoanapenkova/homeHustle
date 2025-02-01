using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameStats : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject endGameScreen;
    [SerializeField]
    private GameObject tasksList;

    [Header("End Game Stats")]
    [SerializeField]
    private Slider timePlayedSlider;
    [SerializeField]
    private TMP_Text earnedPointsText;
    [SerializeField]
    private TMP_Text spentPointsText;
    [SerializeField]
    private GameObject lightsSabotageCheckYes;
    [SerializeField]
    private GameObject lightsSabotageCheckNo;
    [SerializeField]
    private GameObject electricPanelSabotageCheckYes;
    [SerializeField]
    private GameObject electricPanelSabotageCheckNo;
    [SerializeField]
    private GameObject waterSystemSabotageCheckYes;
    [SerializeField]
    private GameObject waterSystemSabotageCheckNo;
    [SerializeField]
    private GameObject stateTamperingSabotageCheckYes;
    [SerializeField]
    private GameObject stateTamperingSabotageCheckNo;

    public NetworkVariable<int> lostTimeHumans = new NetworkVariable<int>(0);
    public NetworkVariable<int> lostTimeObjects = new NetworkVariable<int>(0);
    public NetworkVariable<bool> manipulatedLights = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> usedElectricPanel = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> alteredWaterSystem = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> tamperedItemsState = new NetworkVariable<bool>(false);

    public int spentPoints = 0;
    private PlayerManager playerManager;

    public static GameStats Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowGameStatsServerRpc()
    {
        endGameScreen.SetActive(true);
        tasksList.transform.SetParent(endGameScreen.transform);

        float timePlayedHumans = (UIManager.Instance.countdownDuration - lostTimeHumans.Value) / (2 * UIManager.Instance.countdownDuration);
        //float timePlayedObjects = 1 - timePlayedHumans;

        timePlayedSlider.value = timePlayedHumans;

        earnedPointsText.text = (playerManager.points + spentPoints).ToString();
        spentPointsText.text = spentPoints.ToString();

        lightsSabotageCheckYes.SetActive(manipulatedLights.Value);
        lightsSabotageCheckNo.SetActive(!manipulatedLights.Value);
        electricPanelSabotageCheckYes.SetActive(usedElectricPanel.Value);
        electricPanelSabotageCheckNo.SetActive(!usedElectricPanel.Value);
        waterSystemSabotageCheckYes.SetActive(alteredWaterSystem.Value);
        waterSystemSabotageCheckNo.SetActive(!alteredWaterSystem.Value);
        stateTamperingSabotageCheckYes.SetActive(tamperedItemsState.Value);
        stateTamperingSabotageCheckNo.SetActive(!tamperedItemsState.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateLostTimeHumansServerRpc(int newValue)
    {
        lostTimeHumans.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateLostTimeObjectsServerRpc(int newValue)
    {
        lostTimeObjects.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateLightsSabotageServerRpc(bool newValue)
    {
        manipulatedLights.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateEPanelSabotageServerRpc(bool newValue)
    {
        usedElectricPanel.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateWaterSabotageServerRpc(bool newValue)
    {
        alteredWaterSystem.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTamperSabotageServerRpc(bool newValue)
    {
        tamperedItemsState.Value = newValue;
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

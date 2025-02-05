using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool gameStarted = false;
    public event Action OnGameStarted;

    public PlayerManager playerManager;

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

    void Start()
    {
        Debug.Log("GameManager Initialized!");
    }

    public bool IsGameActive
    {
        get { return gameStarted; }
        set
        {
            if (!gameStarted && value)
            {
                gameStarted = value;
                Debug.Log("Game is starting!");
                OnGameStarted?.Invoke();
            }
        }
    }

    public void StartGameSession()
    {
        UIManager.Instance.GetHUD();
        TimePowerUpManager.Instance.PrepareSpawnPoints();

        IsGameActive = true;
    }

    public void EndGameSession()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // Disconnect all players
        }

        Debug.Log("Game session ended. Reload in few instants.");
        IsGameActive = false;

        // Close the game window
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in Unity Editor
#else
        Application.Quit(); // Close the built game
#endif
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

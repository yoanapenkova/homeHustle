using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool gameStarted = false;
    public event Action OnGameStarted;

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
                OnGameStarted?.Invoke();
            }
        }
    }

    public void StartGameSession()
    {
        UIManager.Instance.GetHUD();
        TimePowerUpManager.Instance.PrepareSpawnPoints();

        gameStarted = true;
    }

    public void EndGameSession()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // Disconnect all players
        }

        Debug.Log("Game session ended. Reload in few instants.");
        gameStarted = false;

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

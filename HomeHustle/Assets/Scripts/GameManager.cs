using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        
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
}

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClickerMinigame : NetworkBehaviour
{
    [Header("Cost Management")]
    [SerializeField]
    public int costPerObject = 5;

    public PlayerManager playerManager;

    [Header("UI Elements")]
    public Button clickerButton;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public Button exitButton;

    [Header("Game Settings")]
    public float gameDuration = 10f; // Countdown duration in seconds

    public int score = 0;           // Player's score
    private float timeRemaining;    // Remaining time
    private bool isGameActive = false; // Flag to track if the game is running
    private bool hasGameStarted = false; // Flag to detect the first click

    public BedAction bed;

    void Start()
    {
        // Set up button events
        clickerButton.onClick.AddListener(OnClickerButtonPressed);
        exitButton.onClick.AddListener(OnExitButtonPressed);

        // Ensure exit button is initially hidden
        exitButton.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsSpawned) { return; }

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (isGameActive)
        {
            // Update the timer
            timeRemaining -= Time.deltaTime;

            if (timeRemaining > 0)
            {
                // Update the timer text
                timerText.text = $"Time: {timeRemaining:F1}";
            }
            else
            {
                // End the game when the timer runs out
                EndGame();
            }
        }
    }

    public void RefreshGame()
    {
        // Initialize UI
        timeRemaining = gameDuration;
        score = 0;
        scoreText.text = "Score: 0";
        timerText.text = "Ready?";

        hasGameStarted = false;
    }

    void OnClickerButtonPressed()
    {
        bool isAllowed = (playerManager.points - costPerObject) >= 0;

        if (!hasGameStarted)
        {
            if (isAllowed)
            {
                StartGame();
                playerManager.points -= costPerObject;
                GameStats.Instance.spentPoints += costPerObject;
                if (!GameStats.Instance.tamperedItemsState.Value)
                {
                    GameStats.Instance.UpdateTamperSabotageServerRpc(true);
                }
            }
            else
            {
                string message = "Need more energy!";
                UIManager.Instance.ShowFeedback(message);
            }
        }

        if (isGameActive)
        {
            // Increment the score and update the score text
            score++;
            scoreText.text = $"Score: {score}";
        }
    }

    void StartGame()
    {
        // Mark the game as started
        hasGameStarted = true;
        isGameActive = true;

        // Reset the timer
        timeRemaining = gameDuration;

        // Update the timer text
        timerText.text = $"Time: {gameDuration:F1}";
    }

    void EndGame()
    {
        // Stop the game
        isGameActive = false;

        // Show the exit button
        exitButton.gameObject.SetActive(true);

        // Update the timer text to indicate time's up
        timerText.text = "Time's Up!";
    }

    void OnExitButtonPressed()
    {
        // Exit the minigame (e.g., deactivate the UI or load another scene)
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        int randomValue = Random.Range(0, 101);
        if (randomValue < score)
        {
            bed.PerformStateChange();
        } else
        {
            string message = "Didn't work this time buddy, try again!";
            UIManager.Instance.ShowFeedback(message);
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

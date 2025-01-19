using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    [Header("General")]
    public NetworkVariable<int> connectedPlayers = new NetworkVariable<int>(
       0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Pre-game")]
    [SerializeField]
    public int maxPlayers = 4;
    [SerializeField]
    private GameObject homeScreen;
    [SerializeField]
    private GameObject preGameScreen;
    [SerializeField]
    private TMP_Text playersCounterText;
    [SerializeField]
    private TMP_Text startingGameText;
    [SerializeField] private TMP_Text playersNamesText; // Reference to TMP_Text UI element
    private static string playerList = "Players:\n"; // String to hold the list of players

    [Header("In-game")]
    [SerializeField]
    private GameObject hudScreen;
    [SerializeField]
    private TMP_Text countdownTimerHumansText;
    [SerializeField]
    private TMP_Text countdownTimerObjectsText;
    [SerializeField]
    private Slider sliderHumans;
    [SerializeField]
    private Slider sliderObjects;
    [SerializeField]
    private ParticleSystem sparksHumans;
    [SerializeField]
    private ParticleSystem sparksObjects;
    [SerializeField]
    private int countdownDuration = 600;

    public int timeHumans;
    public int timeObjects;

    public static UIManager Instance; // Singleton instance

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate managers
        }
    }

    private void Start()
    {
        // Initially update the player list UI for the current client at the start
        UpdatePlayerListUI(playerList);

        timeHumans = countdownDuration;
        timeObjects = countdownDuration; 
    }

    ///////////////////////////////////////
    ///This is for the pre-screen
    ///////////////////////////////////////

    public void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            // Increment the counter for each new client
            connectedPlayers.Value++;

            // Ensure late-joining client gets the correct value immediately
            SendPlayerCountToClientServerRpc(clientId);
        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Decrement the counter when a client disconnects
            connectedPlayers.Value--;
        }
    }

    // Hook into the client connected callback to detect when a player connects
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedForNames;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedForNames;
    }

    // When a client connects, call OnClientConnected to update the list
    private void OnClientConnectedForNames(ulong clientId)
    {
        if (IsServer) // Only the server should manage the player list
        {
            // Add the new player name to the list
            string newPlayerName = "Player_" + clientId; // Replace this with the actual player name if available
            playerList += newPlayerName + "\n";

            // Send the updated list to all clients
            UpdatePlayerListClientRpc(playerList);
        }
    }

    public void GetPreScreen()
    {
        connectedPlayers.Value++;
        homeScreen.SetActive(false);
        preGameScreen.SetActive(true);
    }

    public void OnPlayerCountChanged(int oldCount, int newCount)
    {
        //Debug.Log("ON PLAYER COUNT CHANGED");
        //Debug.Log("OLD COUNT: " + oldCount);
        //Debug.Log("NEW COUNT: " + newCount);
        // Update the UI text on all clients when the value changes
        playersCounterText.text = $"Players {newCount}/8";

        if(newCount == maxPlayers)
        {
            StartCoroutine(PreparePlayers());
        }
    }

    private IEnumerator PreparePlayers()
    {
        // Show the message
        startingGameText.gameObject.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(3f);

        // Hide the message
        startingGameText.gameObject.SetActive(false);

        // Execute your next action here
        GameManager.Instance.StartGameSession();
    }

    // Custom RPC to send the current count to a specific client
    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerCountToClientServerRpc(ulong clientId)
    {
        PlayerCountClientRpc(connectedPlayers.Value, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
    }

    // ClientRPC to update the text on late-joining clients
    [ClientRpc]
    private void PlayerCountClientRpc(int count, ClientRpcParams clientRpcParams = default)
    {
        playersCounterText.text = $"Players {count}/8";
    }

    // Broadcast the updated player list to all clients
    [ClientRpc]
    private void UpdatePlayerListClientRpc(string updatedList)
    {
        playersNamesText.text = updatedList;
    }

    // Update the player list UI for the local client
    private void UpdatePlayerListUI(string updatedList)
    {
        playersNamesText.text = updatedList;
    }

    ///////////////////////////////////////
    ///This is for the HUD (game screen)
    ///////////////////////////////////////
    ///
    
    public void GetHUD()
    {
        preGameScreen.SetActive(false);
        hudScreen.SetActive(true);

        StartCoroutine(StartCountdownHumans());
        StartCoroutine(StartCountdownObjects());
    }

    private IEnumerator StartCountdownHumans()
    {
        sparksHumans.Play();
        while (timeHumans > 0)
        {
            int minutes = Mathf.FloorToInt(timeHumans / 60); // Calculate the minutes
            int seconds = Mathf.FloorToInt(timeHumans % 60); // Calculate the seconds

            countdownTimerHumansText.text = $"{minutes:00}:{seconds:00}"; // Format as MM:SS
            sliderHumans.value = timeHumans;
            
            yield return new WaitForSeconds(1);
            timeHumans--;
        }
    }

    private IEnumerator StartCountdownObjects()
    {
        sparksObjects.Play();
        while (timeObjects > 0)
        {
            int minutes = Mathf.FloorToInt(timeObjects / 60); // Calculate the minutes
            int seconds = Mathf.FloorToInt(timeObjects % 60); // Calculate the seconds

            countdownTimerObjectsText.text = $"{minutes:00}:{seconds:00}"; // Format as MM:SS
            sliderObjects.value = timeObjects;

            yield return new WaitForSeconds(1);
            timeObjects--;
        }
    }
}

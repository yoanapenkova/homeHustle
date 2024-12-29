using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text playersNamesText; // Reference to TMP_Text UI element
    private static string playerList = "Players:\n"; // String to hold the list of players

    private void Start()
    {
        // Initially update the player list UI for the current client at the start
        UpdatePlayerListUI(playerList);
    }

    // Broadcast the updated player list to all clients
    [ClientRpc]
    private void UpdatePlayerListClientRpc(string updatedList)
    {
        playersNamesText.text = updatedList;
    }

    // Hook into the client connected callback to detect when a player connects
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    // When a client connects, call OnClientConnected to update the list
    private void OnClientConnected(ulong clientId)
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

    // Update the player list UI for the local client
    private void UpdatePlayerListUI(string updatedList)
    {
        playersNamesText.text = updatedList;
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TimePowerUpAction : NetworkBehaviour
{
    private bool isActivated = false;

    public bool collected = false;
    private PlayerManager collidingPlayerObject;

    // Networked variable to track if the power up is collected
    private NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the NetworkVariable's value change event
        isCollected.OnValueChanged += OnCollectedStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is a player
        if (other.tag == "Player")
        {
            
            PlayerManager playerManager = other.gameObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                collidingPlayerObject = playerManager;
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleCollectedState();
        }
        // If we are a client, request the server to toggle the collected state
        else
        {
            ToggleCollectedStateServerRpc();
        }

        gameObject.SetActive(false);
    }

    public void UpdateInstructions()
    {
        throw new System.NotImplementedException();
    }

    // Handles the logic to toggle the power up's collected state
    private void ToggleCollectedState()
    {
        // Toggle the power up's collected state
        isCollected.Value = !isCollected.Value;
        collected = isCollected.Value;

        // Notify all clients that the power up's collected state has changed
        CollectedStateChangedClientRpc(isCollected.Value, collidingPlayerObject.isHuman);
    }

    // ServerRpc: Used by clients to request the server to toggle the power up's collected state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleCollectedStateServerRpc()
    {
        ToggleCollectedState();
    }

    // This method is called when the network variable 'isCollected' changes
    private void OnCollectedStateChanged(bool previousValue, bool newValue)
    {
        collected = newValue;
    }

    // ClientRpc: Used to notify clients of the power up's collected state change
    [ClientRpc]
    private void CollectedStateChangedClientRpc(bool newState, bool collidingIsHuman)
    {
        collected = newState;

        if (newState && !isActivated)
        {
            Debug.Log("Power up has been collected.");

            PlayerManager[] players = FindObjectsOfType<PlayerManager>();

            foreach (PlayerManager player in players)
            {
                if (player.IsOwner) // Check if this is the local player
                {
                    Debug.Log("Local Player GameObject: " + player.gameObject.name);
                    //Debug.Log("Original collider is human: " + collidingIsHuman);
                    //Debug.Log("Am I human: " + player.isHuman);
                    if (collidingIsHuman != player.isHuman)
                    {
                        Debug.Log("Shit by the other team.");
                    }
                    else
                    {
                        Debug.Log("Yay, by my team.");
                    }

                    // Apply penalty to the opposing team
                    if (collidingIsHuman)
                    {
                        UIManager.Instance.timeObjects -= 10;
                        Debug.Log("Resting time from objects.");
                    }
                    else
                    {
                        UIManager.Instance.timeHumans -= 10;
                        Debug.Log("Resting time from humans.");
                    }

                    isActivated = true;

                    break;
                }
            }
        }
    }
    
}

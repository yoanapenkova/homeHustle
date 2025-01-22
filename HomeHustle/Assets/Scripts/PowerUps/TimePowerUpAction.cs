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

    private NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    void Start()
    {
        isCollected.OnValueChanged += OnCollectedStateChanged;
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
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
        if (IsServer)
        {
            ToggleCollectedState();
        }
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

    private void ToggleCollectedState()
    {
        isCollected.Value = !isCollected.Value;
        collected = isCollected.Value;

        CollectedStateChangedClientRpc(isCollected.Value, collidingPlayerObject.isHuman);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleCollectedStateServerRpc()
    {
        ToggleCollectedState();
    }

    private void OnCollectedStateChanged(bool previousValue, bool newValue)
    {
        collected = newValue;
    }

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
                    if (collidingIsHuman)
                    {
                        UIManager.Instance.timeObjects -= 10;
                    }
                    else
                    {
                        UIManager.Instance.timeHumans -= 10;
                    }

                    isActivated = true;
                    break;
                }
            }
        }
    }
    
}

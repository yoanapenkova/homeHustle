using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class TimePowerUpAction : NetworkBehaviour
{
    private bool isActivated = false;

    public bool collected = false;
    private PlayerManager collidingPlayerObject;

    private NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    void Start()
    {
        
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
        ToggleCollectedStateServerRpc();
        gameObject.SetActive(false);
    }

    public void UpdateInstructions()
    {
        throw new System.NotImplementedException();
    }

    private void ToggleCollectedState()
    {
        isCollected.Value = true;
        collected = true;

        if (collected && !isActivated)
        {
            Debug.Log("Power up has been collected.");

            PlayerManager[] players = FindObjectsOfType<PlayerManager>();

            foreach (PlayerManager player in players)
            {
                if (player.IsOwner) // Check if this is the local player
                {
                    if (collidingPlayerObject.GetComponent<PlayerManager>().isHuman)
                    {
                        UIManager.Instance.timeObjects -= 10;
                        GameStats.Instance.UpdateLostTimeObjectsServerRpc(10);
                    }
                    else
                    {
                        UIManager.Instance.timeHumans -= 10;
                        GameStats.Instance.UpdateLostTimeHumansServerRpc(10);
                    }

                    isActivated = true;
                    break;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleCollectedStateServerRpc()
    {
        ToggleCollectedState();
        CollectedStateChangedClientRpc(collidingPlayerObject.isHuman);
    }

    private void OnCollectedStateChanged(bool previousValue, bool newValue)
    {
        collected = true;
    }

    [ClientRpc(RequireOwnership = false)]
    private void CollectedStateChangedClientRpc(bool collidingIsHuman)
    {
        collected = true;

        if (collected && !isActivated)
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
                        GameStats.Instance.UpdateLostTimeObjectsServerRpc(10);
                    }
                    else
                    {
                        UIManager.Instance.timeHumans -= 10;
                        GameStats.Instance.UpdateLostTimeHumansServerRpc(10);
                    }

                    isActivated = true;
                    break;
                }
            }
        }
    }
    
}

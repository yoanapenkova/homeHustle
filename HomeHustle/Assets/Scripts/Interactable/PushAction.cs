using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PushAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Aproach", "Push" };

    [Header("Movement Settings")]
    [SerializeField]
    private float pushSpeed = 2f;
    [SerializeField]
    private float proximityDistance = 1f;

    private Interactable interactable;
    private bool isPushing = false;
    private bool isNear = false;
    private Transform playerTransform;
    private Rigidbody objectRigidbody;
    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        objectRigidbody = GetComponent<Rigidbody>();
        if (objectRigidbody == null)
        {
            Debug.LogError("Pushable object must have a Rigidbody component!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (playerManager != null && interactable.isOnWatch)
        {
            UpdateInstructions();

            playerTransform = playerManager.transform;

            float distance = Vector3.Distance(playerTransform.position, transform.position);

            if (distance <= proximityDistance) //Investigar si invertimos esto
            {
                isNear = true;
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    playerManager.gameObject.GetComponent<Animator>().SetBool("Push",true);
                    playerManager.pushFX.gameObject.SetActive(true);
                    playerManager.pushFX.Play();
                    isPushing = true;
                }
            }
            else
            {
                isNear = false;
                isPushing = false;
                playerManager.pushFX.Stop();
                playerManager.pushFX.gameObject.SetActive(false);
            }
        }
    }

    public void Outcome()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);
        if (isNear)
        {
            interactable.auxInstructionsText.text = actions[1];
            interactable.auxInstructionsText.color = Color.white;
            interactable.auxKey.GetComponent<Image>().color = Color.white;
        }
        else
        {
            interactable.auxInstructionsText.text = actions[0];
            interactable.auxInstructionsText.color = Color.grey;
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
        }
    }

    private void FixedUpdate()
    {
        if (isPushing && playerTransform != null)
        {
            // Send push input to the server
            MoveObjectServerRpc(playerTransform.forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveObjectServerRpc(Vector3 pushDirection)
    {
        Vector3 newPosition = transform.position + pushDirection * pushSpeed * Time.fixedDeltaTime;

        // Update position on the server
        objectRigidbody.MovePosition(newPosition);

        // Sync the new position with all clients
        MoveObjectClientRpc(newPosition);
    }

    [ClientRpc]
    private void MoveObjectClientRpc(Vector3 newPosition)
    {
        // Apply the new position on clients (optional for visual feedback)
        objectRigidbody.MovePosition(newPosition);
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

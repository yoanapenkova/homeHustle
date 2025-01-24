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
    private GameObject player;

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

        if (player == null)
        {
            CheckForNetworkAndPlayerServerRpc();
        }

        if (player != null && interactable.isOnWatch)
        {
            UpdateInstructions();

            playerTransform = player.transform;

            float distance = Vector3.Distance(playerTransform.position, transform.position);

            if (distance <= proximityDistance)
            {
                isNear = true;
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    isPushing = true;
                }
            }
            else
            {
                isNear = false;
                isPushing = false;
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
            interactable.auxKey.GetComponent<Image>().color = Color.white;
        }
        else
        {
            interactable.auxInstructionsText.text = actions[0];
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
    void CheckForNetworkAndPlayerServerRpc()
    {
        if (NetworkManager.Singleton.LocalClientId != null)
        {
            // Get the player object for the specified client ID
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var client))
            {
                player = client.PlayerObject.gameObject;
            }
            else
            {
                Debug.LogError($"Client ID {NetworkManager.Singleton.LocalClientId} not found!");
            }
        }
    }
}

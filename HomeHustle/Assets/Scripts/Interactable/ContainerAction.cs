using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ContainerType
{
    Wardrobe, Drawer, BathroomBasket, WashingMachine
}
public class ContainerAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Aproach", "Store" };

    [SerializeField]
    public GameObject containerInventory;
    [SerializeField]
    private GameObject playerInventory;
    [SerializeField]
    public ContainerType containerType;

    private Interactable interactable;
    private bool isInteracting = false;
    private bool isNear = false;

    private PlayerManager playerManager;

    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (playerManager != null)
        {
            if (interactable.isOnWatch && !isInteracting && playerManager.isHuman)
            {
                UpdateInstructions();

                if (Input.GetKeyDown(KeyCode.E) && isNear)
                {
                    Outcome();
                }
            }
        }
    }
    public void Outcome()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        containerInventory.SetActive(true);
        containerInventory.GetComponent<ContainerInventory>().currentObjectInventory = gameObject;

        foreach (InventorySlot slot in playerInventory.GetComponentsInChildren<InventorySlot>())
        {
            slot.GetComponent<StoreAction>().containerInventory = containerInventory.GetComponent<ContainerInventory>();
            slot.GetComponent<StoreAction>().containerInventorySlots = Enumerable.Range(0, containerInventory.transform.childCount)
            .Select(i => containerInventory.transform.GetChild(i).gameObject)
            .OrderBy(child => child.name)
            .ToArray();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (isNear)
        {
            interactable.mainInstructionsText.text = actions[1];
            interactable.mainKey.GetComponent<Image>().color = Color.white;
        }
        else
        {
            interactable.mainInstructionsText.text = actions[0];
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
        }

        interactable.auxKeyBackground.SetActive(false);
        interactable.mainInstructionsText.color = Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        isNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (containerInventory.activeSelf)
            {
                containerInventory.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            isNear = false;
            containerInventory.GetComponent<ContainerInventory>().currentObjectInventory = null;
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

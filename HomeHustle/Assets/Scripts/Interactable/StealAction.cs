using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StealAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Aproach", "Steal" };

    [Header("UI Management")]
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject[] panelOptions;

    private Transform[] optionsPositions = new Transform[4];
    private InventorySlot[] containerInventorySlots = new InventorySlot[4];

    private Interactable interactable;
    private bool isNear = false;

    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        for (int i = 0; i < panelOptions.Length; i++)
        {
            optionsPositions[i] = panelOptions[i].transform;
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

        if (playerManager != null)
        {
            if (interactable.isOnWatch && !playerManager.isHuman)
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

        gamePanel.SetActive(true);

        for (int i = 0; i < panelOptions.Length; i++)
        {
            PanelOption optionSlot = panelOptions[i].GetComponent<PanelOption>();
            if (containerInventorySlots[i] != null)
            {
                if (containerInventorySlots[i].element != null)
                {
                    optionSlot.element = containerInventorySlots[i].element;
                }
                if (containerInventorySlots[i].elementIcon != null)
                {
                    optionSlot.elementIcon = containerInventorySlots[i].elementIcon;
                }
                optionSlot.associatedSlot = containerInventorySlots[i];
            }
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
        gamePanel.GetComponent<GuessTheCardMinigame>().containerInventory = gameObject.GetComponent<ContainerAction>().containerInventory;

        GameObject[] containerInventoryObjects = Enumerable.Range(0, gameObject.GetComponent<ContainerAction>().containerInventory.transform.childCount)
            .Select(i => gameObject.GetComponent<ContainerAction>().containerInventory.transform.GetChild(i).gameObject)
            .OrderBy(child => child.name)
            .ToArray();

        for (int i = 0; i < containerInventoryObjects.Length; i++)
        {
            containerInventorySlots[i] = containerInventoryObjects[i].GetComponent<InventorySlot>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gamePanel.activeSelf)
            {
                gamePanel.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            isNear = false;
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

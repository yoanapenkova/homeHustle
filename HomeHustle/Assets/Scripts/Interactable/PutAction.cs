using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PutAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private GameObject placeholder;

    [Header("Wash Management")]
    [SerializeField]
    private GameObject washUI;
    [SerializeField]
    private Slider progressSlider;

    [Header("Meal Management")]
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private InventorySlot[] playerInventory;
    [SerializeField]
    private Sprite[] sprites;
    private KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    private KeyCode currentKeyCode = KeyCode.E;
    private Sprite originalSprite;

    [Header("Cost Management")]
    [SerializeField]
    private int costPerObject = 10;

    private PlayerManager playerManager;

    private string[] actions = { "Put" };
    public bool occupied;
    private GameObject placedObject;
    private Interactable interactable;

    private NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        isOccupied.OnValueChanged += OnOccupiedStateChanged;
        originalSprite = interactable.mainKey.GetComponent<Image>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (interactable.isOnWatch)
        {
            UpdateInstructions();

            if (Input.GetKeyDown(currentKeyCode) && !occupied && playerManager.isHuman)
            {
                Outcome();
            }
            placeholder.SetActive(true);
        }
        else
        {
            currentKeyCode = KeyCode.E;
            placeholder.SetActive(false);
        }

        if (placedObject != null)
        {
            CheckObjectPickedUp();
        }
    }

    public void Outcome()
    {
        var result = CheckForItemsInInventoryByTag("Plate");

        if (result.Item1)
        {
            placedObject = playerInventory[result.Item2].element;
            placedObject.GetComponent<MealAction>().enabled = true;
            placedObject.GetComponent<MealAction>().platePlaced = true;
            placedObject.GetComponent<MealAction>().gamePanel = gamePanel;
            placedObject.GetComponent<WashAction>().enabled = true;
            placedObject.GetComponent<WashAction>().washUI = washUI;
            placedObject.GetComponent<WashAction>().progressSlider = progressSlider;
            playerInventory[result.Item2].isDirected = true;
            playerInventory[result.Item2].directedTransform = placeholder.transform;
            playerManager.gameObject.GetComponent<InventoryManagement>().enabled = false;
            playerManager.gameObject.GetComponent<InventoryManagement>().HandleSlotShoot(result.Item2);

            ChangeState();
        } else
        {
            string message = "No plates in the inventory!";
            UIManager.Instance.ShowFeedback(message);
        }
        interactable.mainKey.GetComponent<Image>().sprite = originalSprite;
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.auxKeyBackground.SetActive(false);

        interactable.mainInstructionsText.text = actions[0];
        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;

        var result = CheckForItemsInInventoryByTag("Plate");

        if (result.Item1)
        {
            interactable.mainKey.GetComponent<Image>().sprite = sprites[result.Item2];
            currentKeyCode = keyCodes[result.Item2];
        } else
        {
            interactable.mainKey.GetComponent<Image>().sprite = originalSprite;
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
    }

    void ChangeState()
    {
        if (IsServer)
        {
            ToggleOccupiedState();
        }
        else
        {
            ToggleOccupiedStateServerRpc();
        }
    }

    (bool, int) CheckForItemsInInventoryByTag(string tag)
    {
        bool res = false;
        int index = 0;
        for (int i=0; i<playerInventory.Length; i++)
        {
            if (playerInventory[i] != null)
            {
                if (playerInventory[i].element != null)
                {
                    if (playerInventory[i].element.tag == tag)
                    {
                        res = true;
                        index = i;
                    }
                }
            }
        }

        return (res, index);
    }

    void CheckObjectPickedUp()
    {
        PickUpAction pickUpAction = placedObject.GetComponent<PickUpAction>();
        if (pickUpAction != null)
        {
            if (pickUpAction.pickedUp)
            {
                ChangeState();
                placedObject.GetComponent<MealAction>().platePlaced = false;
                placedObject = null;
            }
        }
    }

    private void ToggleOccupiedState()
    {
        isOccupied.Value = !isOccupied.Value;
        occupied = isOccupied.Value;

        OccupiedStateChangedClientRpc(isOccupied.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleOccupiedStateServerRpc()
    {
        ToggleOccupiedState();
    }

    private void OnOccupiedStateChanged(bool previousValue, bool newValue)
    {
        occupied = newValue;
        interactable.enabled = !newValue;
    }

    [ClientRpc]
    private void OccupiedStateChangedClientRpc(bool newState)
    {
        occupied = newState;
        interactable.enabled = !newState;
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

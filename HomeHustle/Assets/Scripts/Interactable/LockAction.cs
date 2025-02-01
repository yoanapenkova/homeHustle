using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LockAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Lock", "Unlock" };
    public bool locked;
    private int usedCombination;
    private Interactable interactable;
    private DoorAction doorAction;
    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(false);
    private NetworkVariable<int> lockCombination = new NetworkVariable<int>(0);

    [Header("Cost Management")]
    [SerializeField]
    private int costPerHuman = 5;
    [SerializeField]
    private int costPerObject = 8;

    private PlayerManager playerManager;

    [Header("UI Management")]
    [SerializeField]
    private GameObject lockUnlockScreen;
    [SerializeField]
    private TMP_InputField[] lockNumbers;
    [SerializeField]
    private Button cancelButton;
    [SerializeField]
    private Button enterButton;
    [SerializeField]
    private TMP_Text exactMatchesText;
    [SerializeField]
    private TMP_Text partialMatchesText;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        doorAction = GetComponent<DoorAction>();

        isLocked.OnValueChanged += OnLockStateChanged;
        lockCombination.OnValueChanged += OnLockCombinationStateChanged;
    }

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

            if (Input.GetKeyDown(KeyCode.Q) && !doorAction.opened)
            {
                ShowLockScreen();
            }
        }
    }

    public void ShowLockScreen()
    {
        lockUnlockScreen.SetActive(true);

        foreach (TMP_InputField lockNumber in lockNumbers)
        {
            lockNumber.text = "0";
        }

        exactMatchesText.text = "-";
        partialMatchesText.text = "-";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cancelButton.onClick.RemoveAllListeners();
        enterButton.onClick.RemoveAllListeners();

        cancelButton.onClick.AddListener(() => HideLockUnlockScreen());
        enterButton.onClick.AddListener(() => lockUnlock());
    }

    void HideLockUnlockScreen()
    {
        lockUnlockScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void lockUnlock()
    {
        string inputCombination = string.Join("", lockNumbers.Select(input => input.text));
        int inputCombinationInt = int.Parse(inputCombination);

        bool isAllowed = playerManager.isHuman ? ((playerManager.points - costPerHuman) >= 0) : ((playerManager.points - costPerObject) >= 0);

        if (isAllowed)
        {
            if (!locked)
            {
                Debug.Log("Locking...");
                usedCombination = inputCombinationInt;
                locked = true;

                UpdateLockStateServerRpc(locked, usedCombination);
                AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.lockDoor);
                HideLockUnlockScreen();
            }
            else
            {
                if (usedCombination == inputCombinationInt)
                {
                    Debug.Log("Unlocking...");
                    usedCombination = 0;
                    locked = false;

                    UpdateLockStateServerRpc(locked, usedCombination);
                    AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.lockDoor);
                    HideLockUnlockScreen();
                }
                else
                {
                    Debug.Log("Incorrect combination.");
                    ProvideFeedback(inputCombinationInt);
                    AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.lockedDoor);
                }
            }
        } else
        {
            string message = playerManager.isHuman ? "Need more coins!" : "Need more energy!";
            UIManager.Instance.ShowFeedback(message);
        }
    }

    private void ProvideFeedback(int inputCombinationInt)
    {
        string paddedLockCombination = usedCombination.ToString().PadLeft(4, '0');
        string paddedInputCombination = inputCombinationInt.ToString().PadLeft(4, '0');

        var matches = GiveFeedbackOnGuess(paddedLockCombination, paddedInputCombination);
        exactMatchesText.text = matches.exactMatches.ToString();
        partialMatchesText.text = matches.partialMatches.ToString();
    }

    private (int exactMatches, int partialMatches) GiveFeedbackOnGuess(string combination, string guess)
    {
        int exactMatches = 0;
        int partialMatches = 0;

        bool[] combinationUsed = new bool[combination.Length];
        bool[] guessUsed = new bool[guess.Length];

        for (int i = 0; i < combination.Length; i++)
        {
            if (guess[i] == combination[i])
            {
                exactMatches++;
                combinationUsed[i] = true;
                guessUsed[i] = true;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            if (!guessUsed[i])
            {
                for (int j = 0; j < combination.Length; j++)
                {
                    if (!combinationUsed[j] && guess[i] == combination[j])
                    {
                        partialMatches++;
                        combinationUsed[j] = true;
                        break;
                    }
                }
            }
        }

        return (exactMatches, partialMatches);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateLockStateServerRpc(bool lockState, int combination)
    {
        if (IsServer)
        {
            isLocked.Value = lockState;
            lockCombination.Value = combination;
            NotifyClientsLockStateChangedClientRpc(lockState, combination);
        }
        else
        {
            UpdateLockStateServerRpc(lockState, combination);
        }
    }

    [ClientRpc]
    private void NotifyClientsLockStateChangedClientRpc(bool newState, int newCombination)
    {
        locked = newState;
        usedCombination = newCombination;
    }

    public void Outcome()
    {
        throw new NotImplementedException();
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);

        if (locked)
        {
            interactable.auxInstructionsText.text = actions[1];
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
        else
        {
            interactable.auxInstructionsText.text = actions[0];
            interactable.mainKey.GetComponent<Image>().color = Color.white;
            interactable.mainInstructionsText.color = Color.white;
            interactable.auxNeededPoints.text = playerManager.isHuman ? "cost: -" + costPerHuman: "cost: -" + costPerObject;
        }

        if (doorAction.opened)
        {
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
        }
        else
        {
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
        }
    }

    private void OnLockStateChanged(bool previousValue, bool newValue)
    {
        locked = newValue;
    }

    private void OnLockCombinationStateChanged(int previousValue, int newValue)
    {
        usedCombination = newValue;
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

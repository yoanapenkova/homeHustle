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

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        doorAction = GetComponent<DoorAction>();

        // Subscribe to the NetworkVariable's value change event
        isLocked.OnValueChanged += OnLockStateChanged;
        lockCombination.OnValueChanged += OnLockCombinationStateChanged;
    }

    void Update()
    {
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
        Debug.Log($"Showing lock/unlock screen. Used: {usedCombination} Lock: {lockCombination.Value}");
        lockUnlockScreen.SetActive(true);

        foreach (TMP_InputField lockNumber in lockNumbers)
        {
            lockNumber.text = "0";
        }

        exactMatchesText.text = "-";
        partialMatchesText.text = "-";

        // Enable the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear previous listeners before adding new ones
        cancelButton.onClick.RemoveAllListeners();
        enterButton.onClick.RemoveAllListeners();

        cancelButton.onClick.AddListener(() => HideLockUnlockScreen());
        enterButton.onClick.AddListener(() => lockUnlock());
    }

    void HideLockUnlockScreen()
    {
        lockUnlockScreen.SetActive(false);

        // Disable the mouse cursor (optional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void lockUnlock()
    {
        Debug.Log("Entering lockUnlock()");

        string inputCombination = string.Join("", lockNumbers.Select(input => input.text));
        int inputCombinationInt = int.Parse(inputCombination);

        if (!locked)
        {
            Debug.Log("Locking...");
            usedCombination = inputCombinationInt;
            locked = true;

            // Update both the lock combination and lock state on the server
            UpdateLockStateServerRpc(locked, usedCombination);

            HideLockUnlockScreen();
            Outcome();
        }
        else
        {
            if (usedCombination == inputCombinationInt)
            {
                Debug.Log("Unlocking...");
                usedCombination = 0;
                locked = false;

                // Update both the lock combination and lock state on the server
                UpdateLockStateServerRpc(locked, usedCombination);

                HideLockUnlockScreen();
                Outcome();
            }
            else
            {
                Debug.Log("Incorrect combination.");
                ProvideFeedback(inputCombinationInt);
            }
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

        // First pass: Find exact matches
        for (int i = 0; i < combination.Length; i++)
        {
            if (guess[i] == combination[i])
            {
                exactMatches++;
                combinationUsed[i] = true;
                guessUsed[i] = true;
            }
        }

        // Second pass: Find partial matches
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
            UpdateLockStateServerRpc(lockState, combination); // Trigger on the server
        }
    }

    // ClientRPC to notify all clients of the lock state and combination change
    [ClientRpc]
    private void NotifyClientsLockStateChangedClientRpc(bool newState, int newCombination)
    {
        locked = newState;
        usedCombination = newCombination;
    }

    public void Outcome()
    {
        if (IsServer)
        {
            Debug.Log("Server handling the outcome.");
            // Handle any additional server logic here
        }
        else
        {
            Debug.Log("Client requesting the outcome.");
            OutcomeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OutcomeServerRpc()
    {
        // Add additional server-side logic if necessary
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
}

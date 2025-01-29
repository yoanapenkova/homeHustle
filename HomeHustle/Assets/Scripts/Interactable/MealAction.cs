using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MealAction : NetworkBehaviour, SimpleAction
{
    public bool platePlaced = false;

    public GameObject gamePanel;

    private string[] actions = { "Make a sandwich" };
    public bool eaten;

    private Interactable interactable;

    private NetworkVariable<bool> isEaten = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        isEaten.OnValueChanged += OnEatenStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch && !eaten && platePlaced && !GameObject.Find("TaskFeedbackManager").GetComponent<TaskFeedbackManager>().breakfastSubstep)
        {
            UpdateInstructions();

            if (Input.GetKeyUp(KeyCode.Q))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        gamePanel.SetActive(true);
        gamePanel.gameObject.GetComponent<SandwichMinigame>().mealAction = this;
        gamePanel.gameObject.GetComponent<SandwichMinigame>().RestartGame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);

        interactable.auxKeyBackground.SetActive(true);
        interactable.auxKey.GetComponent<Image>().color = Color.white;
        interactable.auxInstructionsText.text = actions[0];
        interactable.auxInstructionsText.color = Color.white;
    }

    public void UpdateState()
    {
        if (IsServer)
        {
            ToggleEatenState();
        }
        else
        {
            ToggleEatenStateServerRpc();
        }
    }

    private void ToggleEatenState()
    {
        isEaten.Value = !isEaten.Value;
        eaten = isEaten.Value;

        EatenStateChangedClientRpc(isEaten.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleEatenStateServerRpc()
    {
        ToggleEatenState();
    }

    private void OnEatenStateChanged(bool previousValue, bool newValue)
    {
        eaten = newValue;
    }

    [ClientRpc]
    private void EatenStateChangedClientRpc(bool newState)
    {
        eaten = newState;
    }
}

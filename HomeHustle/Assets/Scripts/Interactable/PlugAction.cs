using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlugAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Connect" };

    [SerializeField]
    private GameObject lightsManagementPanel;
    [SerializeField]
    private Button cancelButton;

    private Interactable interactable;
    private PowerAction powerAction;
    private bool panelOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        powerAction = GetComponent<PowerAction>();
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch && !panelOpened)
        {
            UpdateInstructions();
            // Allow any client to trigger plug actions
            if (Input.GetKeyDown(KeyCode.E) && powerAction.powered)
            {
                Outcome();
            } else
            {
                //TODO: show message that there's no power.
            }
        }
    }

    public void Outcome()
    {
        // Enable the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        lightsManagementPanel.SetActive(true);

        // Clear previous listeners before adding new ones
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => HideLightsManagementPanel());
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.mainInstructionsText.text = actions[0];

        interactable.auxKeyBackground.SetActive(false);

        if (!powerAction.powered)
        {
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
        else
        {
            interactable.mainKey.GetComponent<Image>().color = Color.white;
            interactable.mainInstructionsText.color = Color.white;
        }
    }

    void HideLightsManagementPanel()
    {
        lightsManagementPanel.SetActive(false);

        // Disable the mouse cursor (optional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

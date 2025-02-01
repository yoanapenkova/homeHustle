using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlugAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Connect" };

    [Header("UI Management")]
    [SerializeField]
    private GameObject lightsManagementPanel;
    [SerializeField]
    private Button cancelButton;

    private Interactable interactable;
    private PowerAction powerAction;
    private bool panelOpened = false;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        powerAction = GetComponent<PowerAction>();
    }

    void Update()
    {
        if (interactable.isOnWatch && !panelOpened)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E) && powerAction.powered)
            {
                Outcome();
            } else
            {
                string message = "Plugs have no power!";
                UIManager.Instance.ShowFeedback(message);
            }
        }
    }

    public void Outcome()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        lightsManagementPanel.SetActive(true);
        
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

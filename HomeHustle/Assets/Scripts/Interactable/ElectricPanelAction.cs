using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ElectricPanelAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Connect" };

    [Header("UI Management")]
    [SerializeField]
    private GameObject electricPanelManagementPanel;
    [SerializeField]
    private Button cancelButton;

    private Interactable interactable;
    private bool panelOpened = false;

    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    void Update()
    {
        if (interactable.isOnWatch && !panelOpened)
        {
            UpdateInstructions();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        electricPanelManagementPanel.SetActive(true);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => HideEPManagementPanel());
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.mainInstructionsText.text = actions[0];

        interactable.auxKeyBackground.SetActive(false);
        interactable.mainKey.GetComponent<Image>().color = Color.white;
        interactable.mainInstructionsText.color = Color.white;
    }

    void HideEPManagementPanel()
    {
        electricPanelManagementPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

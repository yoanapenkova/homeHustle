using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ElectricPanelAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = { "Connect" };

    [SerializeField]
    private GameObject electricPanelManagementPanel;
    [SerializeField]
    private Button cancelButton;

    private Interactable interactable;
    private bool panelOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch && !panelOpened)
        {
            UpdateInstructions();
            // Allow any client to trigger boiler actions
            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        // Enable the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        electricPanelManagementPanel.SetActive(true);

        // Clear previous listeners before adding new ones
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

        // Disable the mouse cursor (optional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

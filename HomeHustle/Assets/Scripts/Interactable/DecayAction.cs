using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ProductCondition
{
    Perfect, Normal, Bad
}

public class DecayAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private TMP_Text timeLeftText;
    [SerializeField]
    private TMP_Text conditionText;
    [SerializeField]
    private int timeToSpoil = 120;

    private string[] actions = { "Spoil" };
    public bool altered;
    private ProductCondition condition;
    private Interactable interactable;
    private Camera mainCamera;

    // Networked variable to track if the product is altered
    private NetworkVariable<bool> isAltered = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        mainCamera = Camera.main;

        // Subscribe to the NetworkVariable's value change event
        isAltered.OnValueChanged += OnAlteredStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isOnWatch)
        {
            UpdateInstructions();
            // Allow any client to trigger spoil actions
            if (Input.GetKeyDown(KeyCode.Q) && !altered)
            {
                Outcome();
            }
            timeLeftText.gameObject.SetActive(true);
            conditionText.gameObject.SetActive(true);
        } else
        {
            timeLeftText.gameObject.SetActive(false);
            conditionText.gameObject.SetActive(false);
        }
        UpdateGUI();
    }

    // Ensure that NetworkVariable changes are propagated to clients
    private void OnEnable()
    {
        // Ensure that the state is synced with clients when the product is altered
        if (IsServer)
        {
            // When the server enables the object, initialize the product's state
            isAltered.Value = altered;
        }
    }

    // Unsubscribe when the script is disabled to prevent memory leaks
    private void OnDisable()
    {
        isAltered.OnValueChanged -= OnAlteredStateChanged;
    }

    void UpdateCondition()
    {
        if (timeToSpoil > 75)
        {
            condition = ProductCondition.Perfect;
            conditionText.text = "Perfect";
            conditionText.color = Color.green;
        } else if (timeToSpoil > 15)
        {
            condition = ProductCondition.Normal;
            conditionText.text = "Normal";
            conditionText.color = Color.yellow;
        } else
        {
            condition = ProductCondition.Bad;
            conditionText.text = "Bad";
            conditionText.color = Color.red;
        }

    }

    void UpdateGUI()
    {
        timeLeftText.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        conditionText.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void Outcome()
    {
        // If we are the server, we handle the state change
        if (IsServer)
        {
            ToggleAlteredState();
        }
        // If we are a client, request the server to toggle the product state
        else
        {
            ToggleAlteredStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);
        interactable.auxInstructionsText.text = actions[0];

        if (altered)
        {
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
        } else
        {
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
        }
    }

    // Handles the logic to toggle the product's state
    private void ToggleAlteredState()
    {
        // Toggle the products's altered state
        isAltered.Value = !isAltered.Value;
        altered = isAltered.Value;

        // Notify all clients that the product state has changed
        AlteredStateChangedClientRpc(isAltered.Value);
    }

    // ServerRpc: Used by clients to request the server to toggle the product state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleAlteredStateServerRpc()
    {
        ToggleAlteredState();
    }

    // This method is called when the network variable 'isAltered' changes
    private void OnAlteredStateChanged(bool previousValue, bool newValue)
    {
        altered = newValue;
    }

    // ClientRpc: Used to notify clients of the product state change
    [ClientRpc]
    private void AlteredStateChangedClientRpc(bool newState)
    {
        altered = newState;
        StartCoroutine(ShowTime());
    }

    private IEnumerator ShowTime()
    {
        while (timeToSpoil > 0)
        {
            UpdateCondition();
            int minutes = Mathf.FloorToInt(timeToSpoil / 60); // Calculate the minutes
            int seconds = Mathf.FloorToInt(timeToSpoil % 60); // Calculate the seconds
            timeLeftText.text = $"{minutes:00}:{seconds:00}"; // Format as MM:SS

            yield return new WaitForSeconds(1);
            timeToSpoil--;
        }
    }
}

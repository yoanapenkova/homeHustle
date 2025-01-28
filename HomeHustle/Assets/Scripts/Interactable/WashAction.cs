using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WashAction : NetworkBehaviour, SimpleAction
{
    [Header("UI Management")]
    [SerializeField]
    public GameObject washUI;
    [SerializeField]
    public Slider progressSlider;
    [SerializeField]
    private float interactionSpeed = 1f;
    [SerializeField]
    private float targetProgress = 10f;
    private float interactionProgress = 0f;

    private string[] actions = { "Hold to wash", "Open the tap" };
    public bool washed = false;
    private Interactable interactable;
    private SinkAction sink;

    public bool insideSink = false;
    private bool actionCompleted = false;

    private NetworkVariable<bool> isWashed = new NetworkVariable<bool>(false);

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        isWashed.OnValueChanged += OnWashStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        MealAction mealAction = GetComponent<MealAction>();
        if (mealAction.eaten)
        {
            if (insideSink && !actionCompleted)
            {
                UpdateInstructions();

                if (Input.GetKey(KeyCode.Q) && sink.open)
                {
                    Outcome();
                }
            }
            else
            {
                washUI.SetActive(false);
                interactionProgress = 0f;
                progressSlider.value = interactionProgress;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Object collided with: {other.gameObject.name}");
        SinkAction sinkAction = other.gameObject.GetComponent<SinkAction>();
        if (sinkAction != null )
        {
            sink = sinkAction;
            insideSink = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited the trigger!");
        SinkAction sinkAction = other.gameObject.GetComponent<SinkAction>();
        if (sinkAction != null)
        {
            sink = null;
            insideSink = false;
        }
    }

    public void Outcome()
    {
        interactionProgress += interactionSpeed * Time.deltaTime;
        interactionProgress = Mathf.Clamp(interactionProgress, 0f, targetProgress);
        progressSlider.value = interactionProgress;

        if (interactionProgress >= targetProgress && !actionCompleted)
        {
            UpdateState();
            actionCompleted = true;
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.auxKeyBackground.SetActive(true);
        if (sink.open)
        {
            interactable.auxInstructionsText.text = actions[0];
            interactable.auxKey.GetComponent<Image>().color = Color.white;
            interactable.auxInstructionsText.color = Color.white;
        }
        else
        {
            interactable.auxInstructionsText.text = actions[1];
            interactable.auxKey.GetComponent<Image>().color = Color.grey;
            interactable.auxInstructionsText.color = Color.grey;
        }
        washUI.SetActive(true);
    }

    public void UpdateState()
    {
        if (IsServer)
        {
            ToggleWashState();
        }
        else
        {
            ToggleWashStateServerRpc();
        }
    }

    private void ToggleWashState()
    {
        isWashed.Value = !isWashed.Value;
        washed = isWashed.Value;

        WashStateChangedClientRpc(isWashed.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleWashStateServerRpc()
    {
        ToggleWashState();
    }

    private void OnWashStateChanged(bool previousValue, bool newValue)
    {
        washed = newValue;
    }

    [ClientRpc]
    private void WashStateChangedClientRpc(bool newState)
    {
        washed = newState;
    }
}

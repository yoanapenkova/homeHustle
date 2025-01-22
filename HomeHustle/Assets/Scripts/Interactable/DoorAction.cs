using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DoorAction : NetworkBehaviour, SimpleAction
{
    private string[] actions = {"Open", "Close"};
    public bool opened = false;
    private Animator animator;
    private Interactable interactable;
    private LockAction lockAction;
    private PowerAction parentPowerAction;

    private NetworkVariable<bool> isOpening = new NetworkVariable<bool>(false);

    void Start()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();
        lockAction = GetComponent<LockAction>();
        if (gameObject.transform.parent != null)
        {
            parentPowerAction = gameObject.transform.parent.gameObject.GetComponent<PowerAction>();
        }
        
        isOpening.OnValueChanged += OnDoorStateChanged;
    }

    void Update()
    {
        if (interactable.isOnWatch)
        {
            bool hasPower = true;
            if (parentPowerAction != null)
            {
                hasPower = parentPowerAction.powered;
            }

            UpdateInstructions();

            if (Input.GetKeyDown(KeyCode.E) && hasPower) {
                if (lockAction != null)
                {
                    if (!lockAction.locked)
                    {
                        Outcome(); 
                    } else if (lockAction.locked)
                    {
                        //TODO: Incluir ruido de prohibido al intentar abrir una puerta bloqueada.
                    }
                } else
                {
                    Outcome();
                }
            }
        }
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            isOpening.Value = opened;
        }
    }

    private void OnDisable()
    {
        isOpening.OnValueChanged -= OnDoorStateChanged;
    }

    public void Outcome()
    {
        if (IsServer)
        {
            ToggleDoorState();
        }
        else
        {
            ToggleDoorStateServerRpc();
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        if (opened)
        {
            interactable.mainInstructionsText.text = actions[1];
        }
        else
        {
            interactable.mainInstructionsText.text = actions[0];
        }

        if (lockAction == null)
        {
            interactable.auxKeyBackground.SetActive(false);
        }

        bool hasPower = true;
        if (parentPowerAction != null)
        {
            hasPower = parentPowerAction.powered;
        }

        if (!hasPower)
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

    private void ToggleDoorState()
    {
        isOpening.Value = !isOpening.Value;
        opened = isOpening.Value;

        DoorStateChangedClientRpc(isOpening.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorStateServerRpc()
    {
        ToggleDoorState();
    }

    private void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        animator.SetBool("isOpening", newValue);
    }

    [ClientRpc]
    private void DoorStateChangedClientRpc(bool newState)
    {
        animator.SetBool("isOpening", newState);
    }

    
}

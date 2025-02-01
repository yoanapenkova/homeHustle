using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isOnWatch = false;
    public bool enabled = true;

    [Header("UI Management")]
    [SerializeField]
    public GameObject actionsInstructions;
    [SerializeField]
    public GameObject mainKeyBackground;
    [SerializeField]
    public GameObject mainKey;
    [SerializeField]
    public TMP_Text mainInstructionsText;
    [SerializeField]
    public TMP_Text mainNeededPoints;
    [SerializeField]
    public GameObject auxKeyBackground;
    [SerializeField]
    public GameObject auxKey;
    [SerializeField]
    public TMP_Text auxInstructionsText;
    [SerializeField]
    public TMP_Text auxNeededPoints;


    [Header("Permissions")]
    [SerializeField]
    public PlayerRole[] playerRoles;

    [Header("Object Properties")]
    [SerializeField]
    public bool throwable = false;
}

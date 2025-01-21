using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isOnWatch = false;
    public bool enabled = true;

    [SerializeField]
    public GameObject actionsInstructions;

    [SerializeField]
    public GameObject mainKeyBackground;

    [SerializeField]
    public GameObject mainKey;

    [SerializeField]
    public TMP_Text mainInstructionsText;

    [SerializeField]
    public GameObject auxKeyBackground;

    [SerializeField]
    public GameObject auxKey;

    [SerializeField]
    public TMP_Text auxInstructionsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

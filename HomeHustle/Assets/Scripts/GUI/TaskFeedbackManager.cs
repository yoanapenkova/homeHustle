using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class TaskFeedbackManager : MonoBehaviour
{
    [Header("Tasks UI")]
    [SerializeField]
    private GameObject lightsOffDone;
    [SerializeField]
    private GameObject waterSystemOkDone;
    [SerializeField]
    private TMP_Text bedOkText;
    [SerializeField]
    private GameObject bedOkDone;

    private GameObject[] lightsObj;
    private GameObject[] waterObj;
    private GameObject[] bedsObj;
    public bool lightsTask = false;
    public bool waterTask = true;
    public bool bedSubstep = true;

    void Start()
    {
        lightsObj = GameObject.FindGameObjectsWithTag("Light");
        waterObj = GameObject.FindGameObjectsWithTag("WaterComponent");
        bedsObj = GameObject.FindGameObjectsWithTag("Bed");
    }

    void Update()
    {
        UpdateTaskStats();
        UpdateUI();
    }

    void UpdateTaskStats()
    {

        // Simple Task: Lights Off
        bool lightsTurnedOn = true;
        foreach (GameObject lightObj in lightsObj)
        {
            LightSwitchAction light = lightObj.GetComponent<LightSwitchAction>();
            if (light != null)
            {
                lightsTurnedOn = lightsTurnedOn && !light.turnedOn;
            }
        }
        lightsTask = lightsTurnedOn;

        // Simple Task: Water Care
        bool waterBroken = true;
        foreach (GameObject component in waterObj)
        {
            WaterComponentAction waterComponent = component.GetComponent<WaterComponentAction>();
            if (waterComponent != null)
            {
                waterBroken = waterBroken && !waterComponent.broken;
            }
        }

        waterTask = waterBroken;

        // Complex Task - Substep: Make bed X/3
        bool madeBeds = true;
        int bedsMade = 0;
        foreach (GameObject bedObject in bedsObj)
        {
            BedAction bed = bedObject.GetComponent<BedAction>();
            if (bed != null)
            {
                madeBeds = madeBeds && bed.made;
                if (bed.made)
                {
                    bedsMade++;
                }
            }
        }

        bedSubstep = madeBeds;
        bedOkText.text = "Make bed " + bedsMade + "/3";
        if (bedsMade == 3)
        {
            foreach(GameObject bedObject in bedsObj)
            {
                bedObject.GetComponent<Interactable>().enabled = false;
            }
        }
    }

    void UpdateUI()
    {
        lightsOffDone.SetActive(lightsTask);
        waterSystemOkDone.SetActive(waterTask);
        bedOkDone.SetActive(bedSubstep);
    }
}

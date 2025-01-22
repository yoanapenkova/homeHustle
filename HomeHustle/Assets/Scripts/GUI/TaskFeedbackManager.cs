using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskFeedbackManager : MonoBehaviour
{
    [Header("Tasks UI")]
    [SerializeField]
    private GameObject lightsOffDone;
    [SerializeField]
    private GameObject waterSystemOkDone;

    private GameObject[] lightsObj;
    private GameObject[] waterObj;
    public bool lightsTask = false;
    public bool waterTask = true;

    void Start()
    {
        lightsObj = GameObject.FindGameObjectsWithTag("Light");
        waterObj = GameObject.FindGameObjectsWithTag("WaterComponent");
    }

    void Update()
    {
        UpdateTaskStats();
        UpdateUI();
    }

    void UpdateTaskStats()
    {
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
    }

    void UpdateUI()
    {
        lightsOffDone.SetActive(lightsTask);
        waterSystemOkDone.SetActive(waterTask);
    }
}

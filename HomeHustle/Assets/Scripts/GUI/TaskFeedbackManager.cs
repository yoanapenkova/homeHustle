using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskFeedbackManager : MonoBehaviour
{
    [Header("Tasks UI")]
    [SerializeField]
    private GameObject lightsOffDone;

    private GameObject[] lightsObj;
    public bool lightsTask = false;

    // Start is called before the first frame update
    void Start()
    {
        lightsObj = GameObject.FindGameObjectsWithTag("Light");
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
            //Debug.Log(lightObj.name + ": " + light.turnedOn);
            if (light != null)
            {
                lightsTurnedOn = lightsTurnedOn && !light.turnedOn;
            }
        }
        lightsTask = lightsTurnedOn;
    }

    void UpdateUI()
    {
        lightsOffDone.SetActive(lightsTask);
    }
}

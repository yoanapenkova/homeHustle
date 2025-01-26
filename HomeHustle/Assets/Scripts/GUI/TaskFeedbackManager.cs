using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class TaskFeedbackManager : MonoBehaviour
{
    [Header("Tasks UI")]
    //LIGHTS OFF SIMPLE TASK
    [SerializeField]
    private GameObject lightsOffDone;
    //WATER CARE SIMPLE TASK
    [SerializeField]
    private GameObject waterSystemOkDone;
    //PICK UP ROOM COMPLEX TASK
    //BED MAKING
    [SerializeField]
    private TMP_Text bedOkText;
    [SerializeField]
    private GameObject bedOkDone;
    //STORE CLOTHES
    [SerializeField]
    private GameObject clothesOkDone;
    //DO LAUNDRY
    [SerializeField]
    private GameObject laundryOkDone;

    private GameObject[] lightsObj;
    private GameObject[] waterObj;
    private GameObject[] bedsObj;
    private GameObject[] clothesObj;
    private List<GameObject> cleanClothes = new List<GameObject>();
    private List<GameObject> dirtyClothes = new List<GameObject>();
    public bool lightsTask = false;
    public bool waterTask = true;
    public bool bedSubstep = true;
    public bool clothesSubstep = true;
    public bool laundrySubstep = true;

    void Start()
    {
        lightsObj = GameObject.FindGameObjectsWithTag("Light");
        waterObj = GameObject.FindGameObjectsWithTag("WaterComponent");
        bedsObj = GameObject.FindGameObjectsWithTag("Bed");
        clothesObj = GameObject.FindGameObjectsWithTag("Clothes");
        foreach (GameObject cloth in clothesObj)
        {
            PickUpAction item = cloth.GetComponent<PickUpAction>();
            if (item != null)
            {
                if (item.status == Status.Clean)
                {
                    cleanClothes.Add(cloth);
                } else
                {
                    dirtyClothes.Add(cloth);
                }
            }
        }
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

        // 1.Complex Task - Substep: Make bed X/3
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

        // 1.Complex Task - Substep: Store Clothes
        bool clothesStored = true;
        foreach (GameObject clothObject in cleanClothes)
        {
            bool itemStoredCorrectly = false;
            PickUpAction item = clothObject.GetComponent<PickUpAction>();
            if (item != null && clothObject.transform.parent != null)
            {
                ContainerAction container = clothObject.transform.parent.gameObject.GetComponent<ContainerAction>();
                if (container != null)
                {
                    if (container.containerType == ContainerType.Wardrobe || container.containerType == ContainerType.Drawer)
                    {
                        itemStoredCorrectly = true;
                    }
                }
            }

            clothesStored = clothesStored && itemStoredCorrectly;
        }

        clothesSubstep = clothesStored;

        // 1.Complex Task - Substep: Store Clothes
        bool laundryDone = true;
        foreach (GameObject clothObject in dirtyClothes)
        {
            bool itemStoredCorrectly = false;
            PickUpAction item = clothObject.GetComponent<PickUpAction>();
            if (item != null && clothObject.transform.parent != null)
            {
                ContainerAction container = clothObject.transform.parent.gameObject.GetComponent<ContainerAction>();
                if (container != null)
                {
                    if (container.containerType == ContainerType.BathroomBasket || container.containerType == ContainerType.WashingMachine)
                    {
                        itemStoredCorrectly = true;
                    }
                }
            }

            laundryDone = laundryDone && itemStoredCorrectly;
        }

        laundrySubstep = laundryDone;
    }

    void UpdateUI()
    {
        lightsOffDone.SetActive(lightsTask);
        waterSystemOkDone.SetActive(waterTask);
        bedOkDone.SetActive(bedSubstep);
        clothesOkDone.SetActive(clothesSubstep);
        laundryOkDone.SetActive(laundrySubstep);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

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
    //BREAKFAST COMPLEX TASK
    //SET TABLE
    [SerializeField]
    private GameObject setTableDone;
    //EAT BREAKFAST X/4
    [SerializeField]
    private TMP_Text breakfastText;
    [SerializeField]
    private GameObject breakfastDone;
    //CLEAN DISHES
    [SerializeField]
    private GameObject cleanDishesDone;
    //GET READY COMPLEX TASK
    //HAVE SHOWER
    [SerializeField]
    private TMP_Text showerText;
    [SerializeField]
    private GameObject showerDone;
    //FRESH UP
    [SerializeField]
    private TMP_Text freshUpText;
    [SerializeField]
    private GameObject freshUpDone;

    private GameObject[] lightsObj;
    private GameObject[] waterObj;
    private GameObject[] bedsObj;
    private GameObject[] clothesObj;
    private GameObject[] placematsObj;
    private GameObject[] platesObj;
    private GameObject showerObj;
    private GameObject mirrorObj;
    private List<GameObject> cleanClothes = new List<GameObject>();
    private List<GameObject> dirtyClothes = new List<GameObject>();
    public bool lightsTask = false;
    public bool waterTask = true;
    public bool bedSubstep = true;
    public bool clothesSubstep = true;
    public bool laundrySubstep = true;
    public bool tableSubstep = false;
    public bool breakfastSubstep = false;
    public bool dishesSubstep = false;
    public bool showerSubstep = true;
    public bool freshUpSubstep = true;

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
        placematsObj = GameObject.FindGameObjectsWithTag("Placemat");
        showerObj = GameObject.FindGameObjectWithTag("Shower");
        mirrorObj = GameObject.FindGameObjectWithTag("Mirror");
    }

    void Update()
    {
        platesObj = GameObject.FindGameObjectsWithTag("Plate");
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

        // 2.Complex Task - Substep: Set Table
        if (!tableSubstep)
        {
            bool tableDone = true;
            foreach (GameObject placemat in placematsObj)
            {
                PutAction item = placemat.GetComponent<PutAction>();
                if (item != null)
                {
                    tableDone = tableDone && item.occupied;
                }
            }
            tableSubstep = tableDone;
        }

        // 2.Complex Task - Substeps: Breakfast X/4 and Clean dishes
        bool breakfast = false;
        bool washed = false;
        int sandwiches = 0;
        if (platesObj != null)
        {
            if (platesObj.Length > 0)
            {
                breakfast = true;
                washed = true;
                foreach (GameObject plate in platesObj)
                {
                    bool meal = false;
                    bool wash = false;
                    MealAction mealAction = plate.GetComponent<MealAction>();
                    WashAction washAction = plate.GetComponent<WashAction>();
                    if (mealAction.enabled && washAction.enabled)
                    {
                        meal = mealAction.eaten;
                        if (mealAction.eaten)
                        {
                            sandwiches++;
                        }
                        wash = washAction.washed;
                    }
                    breakfast = breakfast && meal;
                    washed = washed && wash;
                }
            }
        }

        breakfastText.text = "Sandwich " + sandwiches + "/4";

        if (platesObj.Length >= 4)
        {
            if (sandwiches == 4)
            {
                breakfastSubstep = true;
            }
            else
            {
                breakfastSubstep = breakfast;
            }
            dishesSubstep = washed;
        }

        // 3.Complex Task - Substep: Shower X/4
        bool showered = true;
        int showeredHumans = 0;

        ShowerAction shower = showerObj.GetComponent<ShowerAction>();
        if (shower != null)
        {
            if (shower.dadDone) { showeredHumans++; showered = showered && shower.dadDone; } else { showered = false; };
            if (shower.momDone) { showeredHumans++; showered = showered && shower.momDone; } else { showered = false; };
            if (shower.boyDone) { showeredHumans++; showered = showered && shower.boyDone; } else { showered = false; };
            if (shower.girlDone) { showeredHumans++; showered = showered && shower.girlDone; } else { showered = false; };
        }

        showerSubstep = showered;
        showerText.text = "Shower " + showeredHumans + "/4";

        // 3.Complex Task - Substep: Fresh up X/4
        bool freshedUp = true;
        int freshedUpHumans = 0;

        FreshUpAction mirror = mirrorObj.GetComponent<FreshUpAction>();
        if (mirror != null)
        {
            if (mirror.dadDone) { freshedUpHumans++; freshedUp = freshedUp && mirror.dadDone; } else { freshedUp = false; };
            if (mirror.momDone) { freshedUpHumans++; freshedUp = freshedUp && mirror.momDone; } else { freshedUp = false; };
            if (mirror.boyDone) { freshedUpHumans++; freshedUp = freshedUp && mirror.boyDone; } else { freshedUp = false; };
            if (mirror.girlDone) { freshedUpHumans++; freshedUp = freshedUp && mirror.girlDone; } else { freshedUp = false; };
        }

        freshUpSubstep = freshedUp;
        freshUpText.text = "Freshen up " + freshedUpHumans + "/4";
    }

    void UpdateUI()
    {
        lightsOffDone.SetActive(lightsTask);
        waterSystemOkDone.SetActive(waterTask);
        bedOkDone.SetActive(bedSubstep);
        clothesOkDone.SetActive(clothesSubstep);
        laundryOkDone.SetActive(laundrySubstep);
        setTableDone.SetActive(tableSubstep);
        breakfastDone.SetActive(breakfastSubstep);
        cleanDishesDone.SetActive(dishesSubstep);
        showerDone.SetActive(showerSubstep);
        freshUpDone.SetActive(freshUpSubstep);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightAppearancePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject lightSwitch;

    private LightSwitchAction light;
    private Image gameObjImage;
    private Color colorTurnedOn = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Color colorTurnedOff = new Color(0.25f, 0.25f, 0.25f, 1.0f);

    // Start is called before the first frame update
    void Start()
    {
        gameObjImage = GetComponent<Image>();
        light = lightSwitch.GetComponent<LightSwitchAction>();
    }

    // Update is called once per frame
    void Update()
    {
        if (light != null)
        {
            UpdateAppearance();
        }
    }

    void UpdateAppearance()
    {
        if (!light.turnedOn)
        {
            gameObjImage.color = colorTurnedOff;
        }
        else
        {
            gameObjImage.color = colorTurnedOn;
        }
    }
}

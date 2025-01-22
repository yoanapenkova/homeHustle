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

    void Start()
    {
        gameObjImage = GetComponent<Image>();
        light = lightSwitch.GetComponent<LightSwitchAction>();
    }

    void Update()
    {
        if (light != null)
        {
            UpdateAppearance();
        }
    }

    void UpdateAppearance()
    {
        gameObjImage.color = !light.turnedOn ? colorTurnedOff : colorTurnedOn;
    }
}

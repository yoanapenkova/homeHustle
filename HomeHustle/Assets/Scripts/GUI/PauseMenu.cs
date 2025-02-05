using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject controlsScreen;
    [SerializeField]
    private GameObject controlsScreenBackground;
    [SerializeField]
    private GameObject exitControlsScreenButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShowHideMenu();
    }

    void ShowHideMenu()
    {
        if (GameManager.Instance.gameStarted && Input.GetKeyDown(KeyCode.Escape) && !controlsScreen.activeSelf)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            if (pauseMenu.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void HideMenu()
    {
        pauseMenu.SetActive(false);
        if (!controlsScreen.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowHideControls()
    {
        if (GameManager.Instance.gameStarted)
        {
            if (!controlsScreenBackground.activeSelf)
            {
                controlsScreenBackground.SetActive(true);
            }
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            controlsScreen.SetActive(!controlsScreen.activeSelf);
        } else
        {
            controlsScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

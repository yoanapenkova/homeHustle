using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate managers
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameManager Initialized!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGameSession()
    {
        UIManager.Instance.GetHUD();
    }
}

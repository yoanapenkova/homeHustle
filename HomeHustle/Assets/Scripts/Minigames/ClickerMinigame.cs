using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickerMinigame : MonoBehaviour
{
    [Header("UI Elements")]
    public Button clickerButton;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public Button exitButton;

    [Header("Game Settings")]
    public float gameDuration = 10f; // Countdown duration in seconds

    public int score = 0;           // Player's score
    private float timeRemaining;    // Remaining time
    private bool isGameActive = false; // Flag to track if the game is running
    private bool hasGameStarted = false; // Flag to detect the first click

    public BedAction bed;

    void Start()
    {
        // Set up button events
        clickerButton.onClick.AddListener(OnClickerButtonPressed);
        exitButton.onClick.AddListener(OnExitButtonPressed);

        // Ensure exit button is initially hidden
        exitButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameActive)
        {
            // Update the timer
            timeRemaining -= Time.deltaTime;

            if (timeRemaining > 0)
            {
                // Update the timer text
                timerText.text = $"Time: {timeRemaining:F1}";
            }
            else
            {
                // End the game when the timer runs out
                EndGame();
            }
        }
    }

    public void RefreshGame()
    {
        // Initialize UI
        timeRemaining = gameDuration;
        score = 0;
        scoreText.text = "Score: 0";
        timerText.text = "Ready?";

        hasGameStarted = false;
    }

    void OnClickerButtonPressed()
    {
        //TODO: NEED TO FIND THE PLAYERMANAGER AND ADD ACTION COST

        //THE FOLLOWING WOULD HAPPEN ONLY IF THE PLAYER HAS THE ENOUGH POINTS TO DO SO
        if (!hasGameStarted)
        {
            // Start the game on the first click
            StartGame();
        }

        if (isGameActive)
        {
            // Increment the score and update the score text
            score++;
            scoreText.text = $"Score: {score}";
        }
    }

    void StartGame()
    {
        // Mark the game as started
        hasGameStarted = true;
        isGameActive = true;

        // Reset the timer
        timeRemaining = gameDuration;

        // Update the timer text
        timerText.text = $"Time: {gameDuration:F1}";
    }

    void EndGame()
    {
        // Stop the game
        isGameActive = false;

        // Show the exit button
        exitButton.gameObject.SetActive(true);

        // Update the timer text to indicate time's up
        timerText.text = "Time's Up!";
    }

    void OnExitButtonPressed()
    {
        // Exit the minigame (e.g., deactivate the UI or load another scene)
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        int randomValue = Random.Range(0, 101);
        if (randomValue < score)
        {
            bed.PerformStateChange();
        } else
        {
            string message = "Didn't work this time buddy, try again!";
            UIManager.Instance.ShowFeedback(message);
        }
    }
}

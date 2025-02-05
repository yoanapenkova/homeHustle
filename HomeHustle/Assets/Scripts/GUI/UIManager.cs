using System.Collections;
using System.Net.Security;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    [Header("General")]
    public NetworkVariable<int> connectedPlayers = new NetworkVariable<int>(
       0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Pre-game")]
    [SerializeField]
    public int maxPlayers = 4;
    [SerializeField]
    private GameObject homeScreen;
    [SerializeField]
    private GameObject preGameScreen;
    [SerializeField]
    private GameObject controlsScreen;
    [SerializeField]
    private GameObject controlsScreenBackground;
    [SerializeField]
    private TMP_Text playersCounterText;
    [SerializeField]
    private TMP_Text startingGameText;
    [SerializeField] private TMP_Text playersNamesText;
    private static string playerList = "Players:\n";

    [Header("In-game")]
    [SerializeField]
    private GameObject hudScreen;
    private Vector3 hudScreenOriginalScale;
    [SerializeField]
    private TMP_Text countdownTimerHumansText;
    [SerializeField]
    private TMP_Text countdownTimerObjectsText;
    [SerializeField]
    private Slider sliderHumans;
    [SerializeField]
    private Slider sliderObjects;
    [SerializeField]
    public int countdownDuration = 600;
    [SerializeField]
    private TMP_Text feedbackText;
    public float feedbackDisplayDuration = 2f; // Duration to keep the text fully visible
    public float feedbackFadeDuration = 2f;    // Duration for the fading effect
    private Coroutine fadeCoroutine; // Store the current coroutine

    public int timeHumans;
    public int timeObjects;

    public static UIManager Instance;

    private string[] funnyCharacterNames = new string[]{
    "Wobble","Snort","Pickle","Spud","Doofus","Giggles","Noodle","Tater","Fluffy","Wiggles",
    "Dork","Boing","Bungus","Farticus","Bibble","Squishy","Zonk","Bloop","Scoot","Pogo",
    "Bumper","Snazzy","Cheddar","Puddles","Wacky","Toast","Snoozer","Zippy","Gizmo","Goober",
    "Doodles","Scooby","Slinky","Crumbs","Bonk","Skippy","Waffles","Yapper","Banana","Chuckle",
    "Zonkus","Squiggle","Muffin","Blobby","Fizzy","Sprocket","Choco","Twinkle","Bork","Goofus"};

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdatePlayerListUI(playerList);

        timeHumans = countdownDuration;
        timeObjects = countdownDuration;
    }

    private void Update()
    {
        if (timeHumans <= 0 && timeObjects <= 0 && GameManager.Instance.IsGameActive)
        {
            GameManager.Instance.IsGameActive = false;
            GameStats.Instance.ShowGameStats();
        }
    }

    ////////////////////////////////
    ///This is for the pre-screen///
    ////////////////////////////////

    public void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            connectedPlayers.Value++;

            SendPlayerCountToClientServerRpc(clientId);
        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            connectedPlayers.Value--;
        }
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedForNames;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedForNames;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        hudScreenOriginalScale = hudScreen.transform.localScale;
        hudScreen.transform.localScale = Vector3.zero;
    }

    private void OnClientConnectedForNames(ulong clientId)
    {
        if (IsServer)
        {
            string newPlayerName = funnyCharacterNames[Random.Range(0, funnyCharacterNames.Length)] + "#" + clientId.ToString("D4");
            playerList += newPlayerName + "\n";

            UpdatePlayerListClientRpc(playerList);
        }
    }

    public void GetPreScreen()
    {
        connectedPlayers.Value++;
        homeScreen.SetActive(false);
        preGameScreen.SetActive(true);
        controlsScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnPlayerCountChanged(int oldCount, int newCount)
    {
        playersCounterText.text = $"Players {newCount}/8";

        if(newCount == maxPlayers)
        {
            StartCoroutine(PreparePlayers());
        }
    }

    private IEnumerator PreparePlayers()
    {
        startingGameText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        startingGameText.gameObject.SetActive(false);
        GameManager.Instance.StartGameSession();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerCountToClientServerRpc(ulong clientId)
    {
        PlayerCountClientRpc(connectedPlayers.Value, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
    }

    [ClientRpc]
    private void PlayerCountClientRpc(int count, ClientRpcParams clientRpcParams = default)
    {
        playersCounterText.text = $"Players {count}/8";
    }

    [ClientRpc]
    public void UpdatePlayerListClientRpc(string updatedList)
    {
        playersNamesText.text = updatedList;
    }

    private void UpdatePlayerListUI(string updatedList)
    {
        playersNamesText.text = updatedList;
    }

    ///////////////////////////////////////
    ///This is for the HUD (game screen)///
    ///////////////////////////////////////

    public void GetHUD()
    {
        preGameScreen.SetActive(false);
        controlsScreen.SetActive(false);
        controlsScreen.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, controlsScreen.GetComponent<RectTransform>().anchoredPosition.y);
        controlsScreenBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(250, controlsScreenBackground.GetComponent<RectTransform>().anchoredPosition.y);
        hudScreen.transform.localScale = hudScreenOriginalScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(StartCountdownHumans());
        StartCoroutine(StartCountdownObjects());
    }

    private IEnumerator StartCountdownHumans()
    {
        while (true)
        {
            if (timeHumans <=0) { timeHumans = 0; break; }

            int minutes = Mathf.FloorToInt(timeHumans / 60);
            int seconds = Mathf.FloorToInt(timeHumans % 60);

            countdownTimerHumansText.text = $"{minutes:00}:{seconds:00}";
            sliderHumans.value = timeHumans;
            
            yield return new WaitForSeconds(1);
            timeHumans--;
        }
    }

    private IEnumerator StartCountdownObjects()
    {
        while (true)
        {
            if (timeObjects <= 0) { timeObjects = 0; break; }

            int minutes = Mathf.FloorToInt(timeObjects / 60);
            int seconds = Mathf.FloorToInt(timeObjects % 60);

            countdownTimerObjectsText.text = $"{minutes:00}:{seconds:00}";
            sliderObjects.value = timeObjects;

            yield return new WaitForSeconds(1);
            timeObjects--;
        }
    }

    ////////////////////////
    ///Feedback on-screen///
    ////////////////////////

    public void ShowFeedback(string message)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        feedbackText.text = message;
        feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 1f);
        feedbackText.gameObject.SetActive(true);

        fadeCoroutine = StartCoroutine(FadeOutText());
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSeconds(feedbackDisplayDuration);

        float elapsedTime = 0f;
        Color originalColor = feedbackText.color;

        while (elapsedTime < feedbackFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / feedbackFadeDuration);
            feedbackText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        feedbackText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        feedbackText.gameObject.SetActive(false);
        fadeCoroutine = null;
    }
}

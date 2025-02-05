using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Collections;

public class RelayConfig : NetworkBehaviour
{
    private string playerName;

    [Header("Relay Configuration")]
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private TMP_InputField codeInput;

    private string joinCode;

    [Header("UI Management")]
    [SerializeField]
    private TMP_Text joinCodeText;

    [SerializeField]
    private TMP_Text feedbackText;
    public float feedbackDisplayDuration = 2f; // Duration to keep the text fully visible
    public float feedbackFadeDuration = 2f;    // Duration for the fading effect
    private Coroutine fadeCoroutine; // Store the current coroutine

    [Header("Player Prefabs")]
    [SerializeField]
    private GameObject[] playerPrefabs; // Array to store different player prefabs


    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            CreateRelay();
        });
        clientButton.onClick.AddListener(() => {
            JoinRelay(codeInput.text);
        });
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerName);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //For setting up the players' names
        //await AuthenticationService.Instance.UpdatePlayerNameAsync(funnyCharacterNames[Random.Range(0, funnyCharacterNames.Length)]);
    }

    void Update()
    {
        
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(UIManager.Instance.maxPlayers-1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            joinCodeText.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            SpawnPlayerPrefab(NetworkManager.Singleton.LocalClientId);

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += UIManager.Instance.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += UIManager.Instance.OnClientDisconnected;
            UIManager.Instance.connectedPlayers.OnValueChanged += UIManager.Instance.OnPlayerCountChanged;

            UIManager.Instance.GetPreScreen();
        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SpawnPlayerPrefab(clientId);
        }
    }

    private void SpawnPlayerPrefab(ulong clientId)
    {
        int prefabIndex = (int)(clientId % (ulong)playerPrefabs.Length);
        GameObject playerInstance = Instantiate(playerPrefabs[prefabIndex]);

        var networkObject = playerInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId, true);
        }
        else
        {
            Debug.LogError("Assigned prefab does not contain a NetworkObject component.");
        }
    }


    private async void JoinRelay(string joinCode)
    {
        // Check if joinCode is not empty or null
        if (string.IsNullOrEmpty(joinCode))
        {
            // Provide feedback that the input is empty
            ShowFeedback();
            return;
        }

        try
        {
            Debug.Log("Joining Relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            NetworkManager.Singleton.OnClientConnectedCallback += UIManager.Instance.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += UIManager.Instance.OnClientDisconnected;
            UIManager.Instance.connectedPlayers.OnValueChanged += UIManager.Instance.OnPlayerCountChanged;

            UIManager.Instance.GetPreScreen();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            ShowFeedback();
        }
    }

    public void ShowFeedback()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

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

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= UIManager.Instance.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= UIManager.Instance.OnClientDisconnected;
            UIManager.Instance.connectedPlayers.OnValueChanged -= UIManager.Instance.OnPlayerCountChanged;
        }    
    }
}

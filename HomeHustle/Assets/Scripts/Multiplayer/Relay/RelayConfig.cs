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
using Cinemachine;

public class RelayConfig : NetworkBehaviour
{
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

    [Header("Player Prefabs")]
    [SerializeField]
    private GameObject[] playerPrefabs; // Array to store different player prefabs

    private string[] funnyCharacterNames = new string[]{
    "Wobble","Snort","Pickle","Spud","Doofus","Giggles","Noodle","Tater","Fluffy","Wiggles",
    "Dork","Boing","Bungus","Farticus","Bibble","Squishy","Zonk","Bloop","Scoot","Pogo",
    "Bumper","Snazzy","Cheddar","Puddles","Wacky","Toast","Snoozer","Zippy","Gizmo","Goober",
    "Doodles","Scooby","Slinky","Crumbs","Bonk","Skippy","Waffles","Yapper","Banana","Chuckle",
    "Zonkus","Squiggle","Muffin","Blobby","Fizzy","Sprocket","Choco","Twinkle","Bork","Goofus"};


    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            CreateRelay();
            UIManager.Instance.GetPreScreen();
        });
        clientButton.onClick.AddListener(() => {
            JoinRelay(codeInput.text);
            UIManager.Instance.GetPreScreen();
        });
    }

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerName);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //For setting up the players' names
        await AuthenticationService.Instance.UpdatePlayerNameAsync(funnyCharacterNames[Random.Range(0,funnyCharacterNames.Length)]);
    }

    // Update is called once per frame
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

            // Spawn the host's player object
            SpawnPlayerPrefab(NetworkManager.Singleton.LocalClientId);

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += UIManager.Instance.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += UIManager.Instance.OnClientDisconnected;
            UIManager.Instance.connectedPlayers.OnValueChanged += UIManager.Instance.OnPlayerCountChanged;
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
        int prefabIndex = (int)(clientId % (ulong)playerPrefabs.Length); // Cycle through available prefabs
        Debug.Log(prefabIndex);
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
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= UIManager.Instance.OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= UIManager.Instance.OnClientDisconnected;
        UIManager.Instance.connectedPlayers.OnValueChanged -= UIManager.Instance.OnPlayerCountChanged;
    }
}

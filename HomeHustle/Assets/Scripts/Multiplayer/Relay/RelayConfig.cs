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
    private GameObject homeScreen;
    [SerializeField]
    private GameObject preGameScreen;
    [SerializeField]
    private TMP_Text playersCounterText;
    //[SerializeField]
    //private TMP_Text playersNamesTexts;
    [SerializeField]
    private TMP_Text joinCodeText;
    [SerializeField]
    private TMP_Text startingGameText;

    private NetworkVariable<int> connectedPlayers = new NetworkVariable<int>(
       0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
            UpdateUI();
        });
        clientButton.onClick.AddListener(() => {
            JoinRelay(codeInput.text);
            UpdateUI();
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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            joinCodeText.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            connectedPlayers.OnValueChanged += OnPlayerCountChanged;
        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    void UpdateUI()
    {
        connectedPlayers.Value++;
        homeScreen.SetActive(false);
        preGameScreen.SetActive(true);

        Debug.Log("CONNECTED PLAYERS: " + connectedPlayers.Value);

        //playersNamesTexts.text = AuthenticationService.Instance.PlayerName;
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

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            connectedPlayers.OnValueChanged += OnPlayerCountChanged;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        connectedPlayers.OnValueChanged -= OnPlayerCountChanged;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("Hello");
        if (NetworkManager.Singleton.IsClient)
        {
            // Increment the counter for each new client
            connectedPlayers.Value++;

            // Ensure late-joining client gets the correct value immediately
            SendPlayerCountToClientServerRpc(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Decrement the counter when a client disconnects
            connectedPlayers.Value--;
        }
    }

    private void OnPlayerCountChanged(int oldCount, int newCount)
    {
        Debug.Log("ON PLAYER COUNT CHANGED");
        Debug.Log("OLD COUNT: " + oldCount);
        Debug.Log("NEW COUNT: " + newCount);
        // Update the UI text on all clients when the value changes
        playersCounterText.text = $"Players {newCount}/8";
    }

    // Custom RPC to send the current count to a specific client
    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerCountToClientServerRpc(ulong clientId)
    {
        PlayerCountClientRpc(connectedPlayers.Value, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
    }

    // ClientRPC to update the text on late-joining clients
    [ClientRpc]
    private void PlayerCountClientRpc(int count, ClientRpcParams clientRpcParams = default)
    {
        playersCounterText.text = $"Players {count}/8";
    }
}

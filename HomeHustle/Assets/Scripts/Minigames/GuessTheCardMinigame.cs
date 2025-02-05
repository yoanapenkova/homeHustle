using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GuessTheCardMinigame : NetworkBehaviour
{
    [Header("Cost Management")]
    [SerializeField]
    public int costPerObject = 7;

    private PlayerManager playerManager;

    [Header("UI Management")]
    [SerializeField]
    private Button exitPanelButton;
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    public GameObject containerInventory;
    [SerializeField]
    private GameObject[] panelOptions;
    [SerializeField]
    private GameObject[] cards;
    [SerializeField]
    private Button[] cardButtons;

    private Vector3[] originalPositionsCards;
    private Vector3[] originalPositionsOptions;
    private GameObject[] shuffledObjects;

    private bool readyToSelect = false;
    private bool isGameOver = false;


    // Start is called before the first frame update
    void Start()
    {
        // Store original positions of all cards
        originalPositionsCards = new Vector3[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            RectTransform rectTransform = cards[i].GetComponent<RectTransform>();
            originalPositionsCards[i] = rectTransform.anchoredPosition;
        }

        // Store original positions of all cards
        originalPositionsOptions = new Vector3[panelOptions.Length];
        for (int i = 0; i < panelOptions.Length; i++)
        {
            RectTransform rectTransform = panelOptions[i].GetComponent<RectTransform>();
            originalPositionsOptions[i] = rectTransform.anchoredPosition;
        }

        // Add click listeners to all buttons
        foreach (Button button in cardButtons)
        {
            int index = System.Array.IndexOf(cardButtons, button);
            button.onClick.AddListener(() => StartCoroutine(FadeOutButton(button, index)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) { return; }

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameObject.SetActive(false);
    }

    public void RestartUI()
    {
        foreach (GameObject panelOption in panelOptions)
        {
            panelOption.GetComponent<PanelOption>().element = null;
        }

        for (int i = 0; i < panelOptions.Length; i++)
        {
            panelOptions[i].GetComponent<RectTransform>().anchoredPosition = originalPositionsOptions[i];
        }
        foreach (GameObject card in cards)
        {
            Color originalColorCard = cards[0].GetComponent<Image>().color;
            cards[0].GetComponent<Image>().color = new Color(originalColorCard.r, originalColorCard.g, originalColorCard.b, 0);
            card.SetActive(false);
        }
        isGameOver = false;
        startGameButton.gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    public void StartMinigame()
    {
        bool isAllowed = (playerManager.points - costPerObject) >= 0;

        if (isAllowed)
        {
            if (!isGameOver)
            {
                startGameButton.gameObject.SetActive(false);
                StartCoroutine(FadeInFadeOut());
                isGameOver = true;
                playerManager.points -= costPerObject;
                GameStats.Instance.spentPoints += costPerObject;
                if (!GameStats.Instance.tamperedItemsState.Value)
                {
                    GameStats.Instance.UpdateTamperSabotageServerRpc(true);
                }
            }
        } else
        {
            string message = "Need more energy!";
            UIManager.Instance.ShowFeedback(message);
        }
    }

    private IEnumerator FadeInFadeOut()
    {
        cards[0].gameObject.SetActive(true);
        cards[1].gameObject.SetActive(true);
        cards[2].gameObject.SetActive(true);
        cards[3].gameObject.SetActive(true);

        float elapsedTime = 0f;
        Color originalColorCard1 = cards[0].GetComponent<Image>().color;
        Color originalColorCard2 = cards[1].GetComponent<Image>().color;
        Color originalColorCard3 = cards[2].GetComponent<Image>().color;
        Color originalColorCard4 = cards[3].GetComponent<Image>().color;

        while (elapsedTime < 2)
        {
            elapsedTime += Time.deltaTime;
            float alphaCard = Mathf.Lerp(0f, 1f, elapsedTime / 2);
            cards[0].GetComponent<Image>().color = new Color(originalColorCard1.r, originalColorCard1.g, originalColorCard1.b, alphaCard);
            cards[1].GetComponent<Image>().color = new Color(originalColorCard2.r, originalColorCard2.g, originalColorCard2.b, alphaCard);
            cards[2].GetComponent<Image>().color = new Color(originalColorCard3.r, originalColorCard3.g, originalColorCard3.b, alphaCard);
            cards[3].GetComponent<Image>().color = new Color(originalColorCard4.r, originalColorCard4.g, originalColorCard4.b, alphaCard);
            yield return null;
        }

        panelOptions[0].gameObject.SetActive(false);
        panelOptions[1].gameObject.SetActive(false);
        panelOptions[2].gameObject.SetActive(false);
        panelOptions[3].gameObject.SetActive(false);
        StartCoroutine(ShuffleCardsSequence());
    }

    private IEnumerator ShuffleCards(bool moveToZero)
    {
        float duration = 2f; // Duration of the animation
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            for (int i = 0; i < cards.Length; i++)
            {
                RectTransform rectTransform = cards[i].GetComponent<RectTransform>();

                // Use original positions for returning
                Vector3 start = moveToZero ? originalPositionsCards[i] : new Vector3(0f, rectTransform.anchoredPosition.y);
                Vector3 end = moveToZero ? new Vector3(0f, rectTransform.anchoredPosition.y) : originalPositionsCards[i];

                // Lerp between start and end
                rectTransform.anchoredPosition = Vector3.Lerp(start, end, t);
            }

            yield return null;
        }

        // Ensure all cards are at their target positions after the animation
        for (int i = 0; i < cards.Length; i++)
        {
            RectTransform rectTransform = cards[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = moveToZero
                ? new Vector3(0f, rectTransform.anchoredPosition.y)
                : originalPositionsCards[i];
        }

        if (!moveToZero)
        {
            readyToSelect = true;
        }
    }

    private IEnumerator ShuffleCardsSequence()
    {
        // First animation: Move cards to X = 0
        yield return StartCoroutine(ShuffleCards(true));

        RandomizePositions(panelOptions);
        yield return new WaitForSeconds(1);

        // Second animation: Move cards back to their original positions
        yield return StartCoroutine(ShuffleCards(false));

        foreach (GameObject option in panelOptions)
        {
            option.gameObject.SetActive(true);
        }
    }

    public void RandomizePositions(GameObject[] objects)
    {

        // Clone original positions for shuffling
        Vector3[] shuffledPositions = (Vector3[])originalPositionsOptions.Clone();

        // Create a new array to store objects in their randomized order
        shuffledObjects = new GameObject[objects.Length];

        // Perform Fisher-Yates Shuffle for positions and objects
        for (int i = shuffledPositions.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Get a random index within range

            // Swap the positions
            Vector3 tempPosition = shuffledPositions[i];
            shuffledPositions[i] = shuffledPositions[randomIndex];
            shuffledPositions[randomIndex] = tempPosition;

            // Swap the objects to match the shuffled positions
            GameObject tempObject = objects[i];
            objects[i] = objects[randomIndex];
            objects[randomIndex] = tempObject;
        }

        // Store the shuffled objects in the shuffledObjects array
        shuffledObjects = objects;

        // Assign the shuffled positions to the cards (objects)
        for (int i = 0; i < panelOptions.Length; i++)
        {
            panelOptions[i].GetComponent<RectTransform>().anchoredPosition = originalPositionsOptions[i];
        }
        originalPositionsOptions = shuffledPositions;

        // At this point, shuffledObjects holds the objects in their randomized order
        // You can now access shuffledObjects later if needed.
    }

    private IEnumerator FadeOutButton(Button button, int index)
    {
        if (readyToSelect)
        {
            float elapsedTime = 0f;
            Color buttonColor = button.gameObject.GetComponent<Image>().color;

            while (elapsedTime < 2)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2);
                button.gameObject.GetComponent<Image>().color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, alpha);
                yield return null;
            }

            button.gameObject.SetActive(false);
            readyToSelect = false;

            StartCoroutine(PotentialRetrieveItem(shuffledObjects[index].GetComponent<PanelOption>()));
        }
    }

    IEnumerator PotentialRetrieveItem(PanelOption panelOption)
    {
        Debug.Log(panelOption.name);
        if (panelOption.element != null)
        {
            AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.yeah);
            panelOption.associatedSlot.gameObject.GetComponent<RetrieveAction>().RetrieveItem();
            AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.yeah);
        } else
        {
            AudioManager.Instance.PlaySpecificSound(AudioManager.Instance.sadTrombone);
        }
        yield return new WaitForSeconds(1);
        HidePanel();
        RestartUI();
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckForNetworkAndPlayerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject playerObject = client.PlayerObject.gameObject;

            if (playerObject != null)
            {
                AssignPlayerManagerClientRpc(playerObject.GetComponent<NetworkObject>().NetworkObjectId, clientId);
            }
            else
            {
                Debug.LogError($"PlayerManager not found on Client ID {clientId}");
            }
        }
    }

    [ClientRpc]
    void AssignPlayerManagerClientRpc(ulong playerObjectId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectId].gameObject;
            playerManager = playerObject.GetComponent<PlayerManager>();
        }
    }
}

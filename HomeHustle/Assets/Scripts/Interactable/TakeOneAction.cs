using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TakeOneAction : NetworkBehaviour, SimpleAction
{
    [SerializeField]
    private GameObject prefabToSpawn;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private GameObject[] inventorySlots;

    private string[] actions = { "Take plate" };

    private Interactable interactable;
    private GameObject newItem;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (interactable.isOnWatch)
        {
            UpdateInstructions();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Outcome();
            }
        }
    }

    public void Outcome()
    {
        if (spawnPoint.childCount == 0)
        {
            if (IsServer)
            {
                Debug.Log("Server making the request.");
                SpawnNewItem();
            } else
            {
                Debug.Log("Client making the request.");
                SpawnNewItemServerRpc();
            }
            StartCoroutine(PotentialPickUpItem());
        } else
        {
            string message = "There is a plate on the counter!";
            UIManager.Instance.ShowFeedback(message);
        }
    }

    public void UpdateInstructions()
    {
        interactable.actionsInstructions.SetActive(true);
        interactable.mainKeyBackground.SetActive(true);
        interactable.auxKeyBackground.SetActive(false);

        interactable.mainInstructionsText.text = actions[0];
        if (spawnPoint.childCount == 0)
        {
            interactable.mainKey.GetComponent<Image>().color = Color.white;
            interactable.mainInstructionsText.color = Color.white;
        } else
        {
            interactable.mainKey.GetComponent<Image>().color = Color.grey;
            interactable.mainInstructionsText.color = Color.grey;
        }
            
    }
    
    IEnumerator PotentialPickUpItem()
    {
        yield return new WaitForSeconds(.5f);
        if (newItem != null)
        {
            newItem.GetComponent<PickUpAction>().Outcome();
        }
    }

    void SpawnNewItem()
    {
        // Instantiate the object on the server
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        if (networkObject == null)
        {
            Debug.LogError("Prefab does not have a NetworkObject component attached!");
            networkObject.Despawn(true);
            return;
        }

        spawnedObject.transform.parent = spawnPoint.transform;
        spawnedObject.GetComponent<PickUpAction>().inventorySlots = inventorySlots;
        spawnedObject.GetComponent<Interactable>().actionsInstructions = interactable.actionsInstructions;
        spawnedObject.GetComponent<Interactable>().mainKeyBackground = interactable.mainKeyBackground;
        spawnedObject.GetComponent<Interactable>().mainKey = interactable.mainKey;
        spawnedObject.GetComponent<Interactable>().mainInstructionsText = interactable.mainInstructionsText;
        spawnedObject.GetComponent<Interactable>().auxKeyBackground = interactable.auxKeyBackground;
        spawnedObject.GetComponent<Interactable>().auxKey = interactable.auxKey;
        spawnedObject.GetComponent<Interactable>().auxInstructionsText = interactable.auxInstructionsText;
        spawnedObject.GetComponent<PickUpAction>().renderers = spawnedObject.GetComponentsInChildren<Renderer>();
        spawnedObject.GetComponent<PickUpAction>().colliders = spawnedObject.GetComponentsInChildren<Collider>();
        spawnedObject.GetComponent<PickUpAction>().rb = spawnedObject.GetComponent<Rigidbody>();
        newItem = spawnedObject;
        UpdateItemClientRpc(networkObject.NetworkObjectId);
        Debug.Log(newItem);
    }

    [ServerRpc (RequireOwnership = false)]
    void SpawnNewItemServerRpc()
    {
        SpawnNewItem();
    }

    [ClientRpc(RequireOwnership = false)]
    void UpdateItemClientRpc(ulong itemId)
    {
        GameObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId].gameObject;
        spawnedObject.GetComponent<PickUpAction>().inventorySlots = inventorySlots;
        spawnedObject.GetComponent<Interactable>().actionsInstructions = interactable.actionsInstructions;
        spawnedObject.GetComponent<Interactable>().mainKeyBackground = interactable.mainKeyBackground;
        spawnedObject.GetComponent<Interactable>().mainKey = interactable.mainKey;
        spawnedObject.GetComponent<Interactable>().mainInstructionsText = interactable.mainInstructionsText;
        spawnedObject.GetComponent<Interactable>().auxKeyBackground = interactable.auxKeyBackground;
        spawnedObject.GetComponent<Interactable>().auxKey = interactable.auxKey;
        spawnedObject.GetComponent<Interactable>().auxInstructionsText = interactable.auxInstructionsText;
        spawnedObject.GetComponent<PickUpAction>().renderers = spawnedObject.GetComponentsInChildren<Renderer>();
        spawnedObject.GetComponent<PickUpAction>().colliders = spawnedObject.GetComponentsInChildren<Collider>();
        spawnedObject.GetComponent<PickUpAction>().rb = spawnedObject.GetComponent<Rigidbody>();
        newItem = spawnedObject;
    }
}

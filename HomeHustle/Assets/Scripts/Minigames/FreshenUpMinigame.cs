using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FreshenUpMinigame : NetworkBehaviour
{
    [SerializeField]
    private ClothingSum[] sums;
    [SerializeField]
    private FreshUpAction freshUpAction;
    [SerializeField]
    private Image doneIcon;

    private PlayerManager playerManager;

    private bool completed = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;

        if (playerManager == null)
        {
            CheckForNetworkAndPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (freshUpAction == null) return;

        if (!completed)
        {
            CheckForFinish();
        } else
        {
            UpdateCounter();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(false);
    }

    void CheckForFinish()
    {
        bool isItDone = true;
        foreach (ClothingSum item in sums)
        {
            bool sumResult = false;
            GameObject itemInNoColorSlot = item.noColorSlot.GetComponent<ItemSlot>().droppedItem;
            GameObject itemInResultSlot = item.resultSlot.GetComponent<ItemSlot>().droppedItem;
            if (itemInNoColorSlot != null && itemInResultSlot != null)
            {
                sumResult = ((item.color == itemInResultSlot.GetComponent<DragDrop>().color) && (itemInNoColorSlot.GetComponent<DragDrop>().clothType == itemInResultSlot.GetComponent<DragDrop>().clothType));
            }
            isItDone = isItDone && sumResult;
        }

        completed = isItDone;
    }

    void UpdateCounter()
    {
        if (playerManager != null && completed)
        {
            if (playerManager.role == PlayerRole.Dad && !freshUpAction.dadDone)
            {
                freshUpAction.UpdateDad();
            } else if (playerManager.role == PlayerRole.Mom && !freshUpAction.momDone)
            {
                freshUpAction.UpdateMom();
            }
            else if (playerManager.role == PlayerRole.Boy && !freshUpAction.boyDone)
            {
                freshUpAction.UpdateBoy();
            }
            else if (playerManager.role == PlayerRole.Girl && !freshUpAction.girlDone)
            {
                freshUpAction.UpdateGirl();
            }
            freshUpAction.gameObject.GetComponent<Interactable>().enabled = false;
            freshUpAction = null;
            StartCoroutine(ShowDoneIcon());
        }
    }

    IEnumerator ShowDoneIcon()
    {
        doneIcon.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color originalColor = doneIcon.color;

        while (elapsedTime < 2)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / 2);
            doneIcon.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        doneIcon.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        doneIcon.gameObject.SetActive(false);
        doneIcon.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        HidePanel();
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

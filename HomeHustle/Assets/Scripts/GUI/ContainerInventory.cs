using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ContainerInventory : NetworkBehaviour
{
    public GameObject currentObjectInventory;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameObject.SetActive(false);
    }
}

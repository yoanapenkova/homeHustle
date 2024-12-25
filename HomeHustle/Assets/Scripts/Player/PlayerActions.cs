using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerActions : NetworkBehaviour
{
    [SerializeField]
    private Transform objectPrefab;

    private Transform objectInScene;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            spawnObjectServerRpc();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            despawnObjectServerRpc();
        }
    }

    [ServerRpc]
    void spawnObjectServerRpc()
    {
        objectInScene = Instantiate(objectPrefab);
        objectInScene.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc]
    void despawnObjectServerRpc()
    {
        objectInScene.GetComponent<NetworkObject>().Despawn(true);
        Destroy(objectInScene.gameObject);
    }
}

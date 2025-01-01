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

        /*
        if (Input.GetKeyUp(KeyCode.E))
        {
            despawnObjectServerRpc();
        }
        */

        if (Input.GetKeyUp(KeyCode.R))
        {
            getSmallServerRpc();
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

    [ServerRpc]
    void getSmallServerRpc()
    {
        gameObject.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        seeSmallClientRpc();
    }

    [ClientRpc]
    void seeSmallClientRpc()
    {
        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}

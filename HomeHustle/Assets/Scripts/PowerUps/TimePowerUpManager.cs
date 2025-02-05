using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TimePowerUpManager : NetworkBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private Transform powerUpPrefab;

    private bool[] takenSpawnPoints;

    private float spawnInterval = 30f;
    private float powerUpLifetime = 15f;

    public static TimePowerUpManager Instance;

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
        
    }

    public void PrepareSpawnPoints()
    {
        takenSpawnPoints = new bool[spawnPoints.Length];
        if (IsServer)
        {
            StartCoroutine(SpawnPowerUpRoutine());
        }
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int spawnIndex = GetRandomFreeSpawnPoint();
            if (spawnIndex != -1 && GameManager.Instance.gameStarted)
            {
                takenSpawnPoints[spawnIndex] = true;

                Transform spawnPoint = spawnPoints[spawnIndex];
                Transform powerUp = Instantiate(powerUpPrefab, spawnPoint.position, spawnPoint.rotation);
                powerUp.GetComponent<NetworkObject>().Spawn();

                StartCoroutine(HandlePowerUpLifetime(powerUp, spawnIndex));
            }
        }
    }

    private IEnumerator HandlePowerUpLifetime(Transform powerUp, int spawnIndex)
    {
        yield return new WaitForSeconds(powerUpLifetime);

        if (powerUp != null && powerUp.TryGetComponent<NetworkObject>(out var networkObject))
        {
            networkObject.Despawn();
            Destroy(powerUp);

            SubtractTimeBothTeamsClientRpc();
            GameStats.Instance.UpdateLostTimeHumansServerRpc(20);
            GameStats.Instance.UpdateLostTimeObjectsServerRpc(20);
        }

        takenSpawnPoints[spawnIndex] = false;
    }

    private int GetRandomFreeSpawnPoint()
    {
        List<int> freePoints = new List<int>();
        for (int i = 0; i < takenSpawnPoints.Length; i++)
        {
            if (!takenSpawnPoints[i])
            {
                freePoints.Add(i);
            }
        }

        if (freePoints.Count > 0)
        {
            int randomIndex = Random.Range(0, freePoints.Count);
            return freePoints[randomIndex];
        }

        return -1;
    }

    [ClientRpc]
    void SubtractTimeBothTeamsClientRpc()
    {
        UIManager.Instance.timeHumans -= 20;
        UIManager.Instance.timeObjects -= 20;
    }
}

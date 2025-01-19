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

    private float spawnInterval = 30f; // Time between spawns
    private float powerUpLifetime = 10f; // Time before the power-up disappears

    public static TimePowerUpManager Instance; // Singleton instance

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate managers
        }
    }

    private void Start()
    {
        
    }

    public void PrepareSpawnPoints()
    {
        takenSpawnPoints = new bool[spawnPoints.Length];
        if (IsServer) // Ensure only the server manages power-up spawning
        {
            StartCoroutine(SpawnPowerUpRoutine());
        }
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Find a free spawn point
            int spawnIndex = GetRandomFreeSpawnPoint();
            if (spawnIndex != -1) // If a free spawn point is available
            {
                // Mark the spawn point as taken
                takenSpawnPoints[spawnIndex] = true;

                // Spawn the power-up
                Transform spawnPoint = spawnPoints[spawnIndex];
                Transform powerUp = Instantiate(powerUpPrefab, spawnPoint.position, spawnPoint.rotation);
                powerUp.GetComponent<NetworkObject>().Spawn();

                Debug.Log("New power up!!");

                // Start a coroutine to destroy the power-up after a lifetime and free the spawn point
                StartCoroutine(HandlePowerUpLifetime(powerUp, spawnIndex));
            }
        }
    }

    private IEnumerator HandlePowerUpLifetime(Transform powerUp, int spawnIndex)
    {
        yield return new WaitForSeconds(powerUpLifetime);

        // Destroy the power-up and free the spawn point
        if (powerUp != null && powerUp.TryGetComponent<NetworkObject>(out var networkObject))
        {
            networkObject.Despawn(); // Properly despawn the object from the network
            Destroy(powerUp);

            SubtractTimeBothTeamsClientRpc();
        }

        takenSpawnPoints[spawnIndex] = false; // Mark the spawn point as free
    }

    private int GetRandomFreeSpawnPoint()
    {
        // Get all free spawn points
        List<int> freePoints = new List<int>();
        for (int i = 0; i < takenSpawnPoints.Length; i++)
        {
            if (!takenSpawnPoints[i]) // If the spawn point is not taken
            {
                freePoints.Add(i);
            }
        }

        if (freePoints.Count > 0) // If there are free spawn points
        {
            int randomIndex = Random.Range(0, freePoints.Count); // Choose one randomly
            return freePoints[randomIndex];
        }

        return -1; // No free spawn points
    }

    [ClientRpc]
    void SubtractTimeBothTeamsClientRpc()
    {
        UIManager.Instance.timeHumans -= 20;
        UIManager.Instance.timeObjects -= 20;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class PlayerPoints : NetworkBehaviour
{
    [SerializeField]
    public bool isHuman;

    [SerializeField]
    public int points = 0;

    private int incrementAmount = 1;
    private int incrementInterval;
    private GameObject coinsIconObj;
    private GameObject thunderIconObj;
    private TMP_Text pointsText;

    void Start()
    {
        if (!IsOwner) return; // Ensure the script runs only on the owner of the object

        coinsIconObj = GameObject.Find("Coins");
        thunderIconObj = GameObject.Find("Thunder");
        pointsText = GameObject.Find("PointsCounter").GetComponent<TMP_Text>();

        if (isHuman)
        {
            thunderIconObj.SetActive(false);
            incrementInterval = 3;
        }
        else
        {
            coinsIconObj.SetActive(false);
            incrementInterval = 5;
        }

        StartIncrementing();
    }

    void Update()
    {
        if (IsOwner) // Only update UI for the owner
        {
            UpdateCoins();
        }
    }

    private void StartIncrementing()
    {
        StartCoroutine(IncrementPoints());
    }

    private IEnumerator IncrementPoints()
    {
        while (true)
        {
            yield return new WaitForSeconds(incrementInterval);
            if (IsOwner) // Increment points only for the owner
            {
                points += incrementAmount;
            }
        }
    }

    private void UpdateCoins()
    {
        pointsText.text = points.ToString();
    }
}

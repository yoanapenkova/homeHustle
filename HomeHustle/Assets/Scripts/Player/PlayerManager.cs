using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public enum PlayerRole
{
    Dad, Mom, Boy, Girl, Volt, Tidy, Nom, Aqua
}

public class PlayerManager : NetworkBehaviour
{
    [SerializeField]
    public bool isHuman;
    [SerializeField]
    public PlayerRole role;

    [SerializeField]
    public int points = 0;

    private int incrementAmount = 1;
    private int incrementInterval;
    private GameObject coinsIconObj;
    private GameObject thunderIconObj;
    private TMP_Text pointsText;

    [Header("UX/UI")]
    [SerializeField]
    public bool cameraMovement = true;
    [SerializeField]
    private KeyCode[] actionKeys;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (IsOwner)
        {
            UpdateCoins();
        }
        UpdateAnimator();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += SetupPoints;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= SetupPoints;
        }
    }

    void UpdateAnimator()
    {
        foreach (KeyCode key in actionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                animator.SetBool("Interact", true);
            } else if (Input.GetKeyUp(key))
            {
                animator.SetBool("Interact", false);
            }
        }
    }

    private void SetupPoints()
    {
        if (!IsOwner) return;

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

    private void StartIncrementing()
    {
        StartCoroutine(IncrementPoints());
    }

    private IEnumerator IncrementPoints()
    {
        while (true)
        {
            yield return new WaitForSeconds(incrementInterval);
            if (IsOwner)
            {
                points += incrementAmount;
            }
        }
    }

    private void UpdateCoins()
    {
        if (pointsText != null)
        {
            pointsText.text = points.ToString();
        }
    }
}

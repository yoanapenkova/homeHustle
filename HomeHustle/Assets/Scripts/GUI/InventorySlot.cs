using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : NetworkBehaviour
{
    public GameObject element;
    public Image elementIcon;

    [SerializeField]
    public TMP_Text slotTime;
    public float time = 15f;
    public Coroutine shootCoroutine = null;

    private bool occupied;

    public bool isDirected = false;
    public Transform directedTransform;

    void Start()
    {
        
    }

    void Update()
    {
        updateAppearance();
    }

    void updateAppearance()
    {
        if (element != null && !occupied)
        {
            elementIcon = Instantiate(elementIcon);
            elementIcon.gameObject.transform.SetParent(gameObject.transform);

            RectTransform rectTransform = elementIcon.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;

            occupied = true;
        } else if (occupied && element == null)
        {
            Destroy(elementIcon.gameObject);
            elementIcon = null;
            occupied = false;
        }
    }

    public IEnumerator ShootCountdown(int index)
    {
        float timer = time; // Ensure 'time' is set correctly before starting

        if (timer <= 0)
        {
            yield break; // Prevent an infinite loop
        }

        while (timer > 0)
        {
            slotTime.text = timer.ToString(); // Ensure slotTime is assigned!
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        slotTime.text = "";
        
        GameManager.Instance.playerManager.gameObject.GetComponent<InventoryManagement>().HandleSlotShoot(index, false, null);

        shootCoroutine = null; // Reset reference after completion
    }


    public void StartShootCountdown(int index)
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }

        shootCoroutine = StartCoroutine(ShootCountdown(index));
    }


    public void StopShootCountdown()
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
        else
        {
            Debug.LogWarning("[StopShootCountdown] No coroutine running to stop.");
        }
    }
}

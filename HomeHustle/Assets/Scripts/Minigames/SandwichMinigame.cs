using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SandwichMinigame : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text[] ingredientTexts;
    [SerializeField]
    private ItemSlot[] initialSlots;
    [SerializeField]
    private ItemSlot[] resultSlots;
    [SerializeField]
    public MealAction mealAction;
    [SerializeField]
    private Image doneIcon;

    private IngredientType[] order = new IngredientType[4];
    private bool completed = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mealAction == null) return;

        if (!completed)
        {
            if (!mealAction.eaten)
            {
                CheckForFinish();
            }
        }
        else if (!mealAction.eaten) // Prevents multiple calls
        {
            completed = false;
            UpdateData();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(false);
    }

    void CheckForFinish()
    {
        if (completed) { return; }

        bool isItDone = true;

        foreach (ItemSlot slot in resultSlots)
        {
            bool partialResult = false;
            if (slot.droppedItem != null)
            {
                partialResult = slot.waitingFor == slot.droppedItem.GetComponent<DragDrop>().ingredientType;
            }
            isItDone = isItDone && partialResult;
        }

        completed = isItDone;

        if (completed)
        {
            Debug.Log("CheckForFinish: game has been finished.");
            StartCoroutine(ShowDoneIcon());
        }
    }

    void UpdateData()
    {
        if (!mealAction.eaten)
        {
            MealAction meal = mealAction;
            mealAction = null;
            Debug.Log("UpdateData: meal has been eaten.");
            meal.UpdateState();
        }
    }

    public void RestartGame()
    {
        foreach (ItemSlot slot in initialSlots)
        {
            slot.ResetSlot();
            slot.initialItem.GetComponent<DragDrop>().canvasGroup.alpha = 1;
            slot.initialItem.GetComponent<DragDrop>().canvasGroup.blocksRaycasts = true;
        }

        foreach (ItemSlot slot in resultSlots)
        {
            slot.droppedItem = null;
        }

        RandomizeRecipe();
    }

    void RandomizeRecipe()
    {
        List<IngredientType> ingredients = new List<IngredientType>((IngredientType[])Enum.GetValues(typeof(IngredientType)));
        ingredients = ingredients.Take(4).ToList();

        Shuffle(ingredients);

        for (int i = 0; i < ingredientTexts.Length; i++)
        {
            ingredientTexts[i].text = ingredients[i].ToString();
            order[i] = ingredients[i];
            resultSlots[i + 1].GetComponent<ItemSlot>().waitingFor = ingredients[i];
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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
}

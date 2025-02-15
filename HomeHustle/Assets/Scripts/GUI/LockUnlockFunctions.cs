using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockUnlockFunctions : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField lockNumber;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SumOne()
    {
        int currentValue = int.Parse(lockNumber.text);
        if (currentValue < 9 )
        {
            int augmentedValue = currentValue + 1;
            lockNumber.text = augmentedValue.ToString();
        }
    }

    public void RestOne()
    {
        int currentValue = int.Parse(lockNumber.text);
        if (currentValue > 0)
        {
            int reducedValue = currentValue - 1;
            lockNumber.text = reducedValue.ToString();
        }
    }
}

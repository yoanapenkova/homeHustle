using UnityEngine;

public class FloatingAndSpinning : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField]
    private float floatAmplitude = 0.1f;
    [SerializeField]
    private float floatFrequency = 1f;

    [Header("Spinning Settings")]
    [SerializeField]
    private float spinSpeed = 200f;

    private Vector3 startPosition;

    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatFrequency + randomOffset) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, yOffset, 0);

        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
}

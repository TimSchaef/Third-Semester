using UnityEngine;

public class Hover : MonoBehaviour
{
    [Header("Hover Settings")]
    public float amplitude = 0.5f;
    public float frequency = 1f;

    [Header("Phase Offset")]
    public float randomPhaseOffset = 1f;

    private Vector3 startPos;
    private float phase;

    void Start()
    {
        startPos = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f) * randomPhaseOffset;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency * Mathf.PI * 2f + phase) * amplitude;
        transform.position = startPos + Vector3.up * yOffset;
    }
}



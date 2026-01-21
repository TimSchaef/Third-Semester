using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float floatUpSpeed = 1.25f;
    [SerializeField] private Vector3 randomOffset = new Vector3(0.25f, 0.10f, 0.25f);

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 0.9f;

    [Header("Look")]
    [SerializeField] private bool faceCamera = true;

    private TextMeshProUGUI tmp;
    private Color startColor;
    private float timer;

    public void Init(float amount)
    {
        if (!tmp) tmp = GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmp)
        {
            tmp.text = Mathf.RoundToInt(amount).ToString();
            startColor = tmp.color;
        }

        // leichter Random-Offset, damit mehrere Zahlen nicht exakt übereinander liegen
        transform.position += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // nach oben floaten
        transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;

        // zur Kamera drehen (optional)
        if (faceCamera && Camera.main)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        // ausfaden
        if (tmp)
        {
            float t = Mathf.Clamp01(timer / Mathf.Max(0.0001f, lifetime));
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            tmp.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}


using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float floatUpSpeed = 1.25f;
    [SerializeField] private Vector3 randomOffset = new Vector3(0.25f, 0.10f, 0.25f);

    [Header("Time")]
    [SerializeField] private float lifetime = 0.9f;

    [Header("Look")]
    [SerializeField] private bool faceCamera = true;

    [Header("Crit")]
    [SerializeField] private Color critColor = Color.yellow;
    [SerializeField] private float critSizeMultiplier = 2f;

    private TextMeshProUGUI tmp;
    private Color startColor;
    private float timer;
    private float baseFontSize;

    public void Init(float amount, bool isCrit = false)
    {
        if (!tmp) tmp = GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmp)
        {
            tmp.text = Mathf.RoundToInt(amount).ToString();

            if (baseFontSize <= 0f)
                baseFontSize = tmp.fontSize;

            if (isCrit)
            {
                tmp.color = critColor;
                tmp.fontSize = baseFontSize * Mathf.Max(1f, critSizeMultiplier);
            }
            else
            {
                tmp.fontSize = baseFontSize; 
            }

            startColor = tmp.color;
        }
        
        transform.position += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
        
        if (faceCamera && Camera.main)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
        
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


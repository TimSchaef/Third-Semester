using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DamageNumber : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text text;

    [Header("Anim")]
    public float lifetime = 0.8f;
    public Vector3 startOffset = new Vector3(0f, 1.4f, 0f);
    public Vector3 scatter = new Vector3(0.3f, 0.3f, 0.3f); // zufällige Abweichung
    public float riseDistance = 1.2f;

    private float t;
    private Vector3 startPos;
    private Vector3 endPos;
    private CanvasGroup cg;
    private Camera cam;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cam = Camera.main;
    }

    public void Init(string txt, Color color, Transform follow)
    {
        if (text) { text.text = txt; text.color = color; }
        var rnd = new Vector3(Random.Range(-scatter.x, scatter.x), Random.Range(0f, scatter.y), Random.Range(-scatter.z, scatter.z));
        startPos = (follow ? follow.position : transform.position) + startOffset + rnd;
        endPos   = startPos + Vector3.up * riseDistance;
        transform.position = startPos;
    }

    void Update()
    {
        t += Time.deltaTime / Mathf.Max(0.01f, lifetime);
        float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        transform.position = Vector3.Lerp(startPos, endPos, e);

        if (cam) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        cg.alpha = 1f - e;
        if (t >= 1f) Destroy(gameObject);
    }
}


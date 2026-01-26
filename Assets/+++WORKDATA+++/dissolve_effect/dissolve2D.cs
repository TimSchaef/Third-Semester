using System.Collections;
using UnityEngine;

public class dissolve2D : MonoBehaviour
{
    private MaterialPropertyBlock propertyBlock;
    private SpriteRenderer[] spriteRenderers;

    [Header("Auto Play")]
    public bool playOnStart = true;
    public bool isSpawning = false;

    [Header("Timing")]
    public float duration = 0.6f;
    public float delay = 0.6f;

    [Header("Despawn Behaviour")]
    public bool destroyOnDissolve = true;

    private Coroutine running;

    public float TotalTime => Mathf.Max(0f, delay) + Mathf.Max(0.01f, duration);

    private static readonly int VerticalProp = Shader.PropertyToID("_VerticalDissolve");
    private static readonly int DissolveProp = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        // Initial state fix (damit Spawn von "unsichtbar" startet)
        if (isSpawning)
            SetVertical(0f);

        if (playOnStart)
        {
            if (isSpawning) Spawn();
            else Dissolve();
        }
    }

    public void Spawn()
    {
        if (running != null) StopCoroutine(running);

        // Spawn: vertical von 0 -> 1
        SetVertical(0f);

        running = StartCoroutine(SpawnCoroutine());
    }

    public void Dissolve()
    {
        if (running != null) StopCoroutine(running);

        // Despawn: dissolve amount von 0 -> 1
        SetDissolve(0f);

        running = StartCoroutine(DissolveCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float t = 0f;
        float inv = 1f / Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            SetVertical(Mathf.Clamp01(t));
            yield return null;
        }

        running = null;
    }

    private IEnumerator DissolveCoroutine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float t = 0f;
        float inv = 1f / Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            SetDissolve(Mathf.Clamp01(t));
            yield return null;
        }

        running = null;

        if (destroyOnDissolve)
            Destroy(gameObject);
    }

    private void SetVertical(float v)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            sr.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(VerticalProp, v);
            sr.SetPropertyBlock(propertyBlock);
        }
    }

    private void SetDissolve(float v)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            sr.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DissolveProp, v);
            sr.SetPropertyBlock(propertyBlock);
        }
    }
}


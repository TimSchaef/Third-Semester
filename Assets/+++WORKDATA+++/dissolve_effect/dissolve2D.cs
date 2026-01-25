using System.Collections;
using UnityEngine;

public class dissolve2D : MonoBehaviour
{
    private MaterialPropertyBlock PropertyBlock;
    private SpriteRenderer[] spriteRenderers;
    
    public bool playOnStart = true;
    public bool isSpawning = false; 
    public float duration = 0.6f;
    public float delay = 0.6f;

    private void Start()
    {
        
        PropertyBlock = new MaterialPropertyBlock();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (isSpawning)
        {
            for (int i = 0; i < spriteRenderers.Length; ++i)
            {
                PropertyBlock.SetFloat("_VerticalDissolve", 0);
                spriteRenderers[i].SetPropertyBlock(PropertyBlock);
            }
        }
        
        if (playOnStart)
        {
            if (isSpawning)
                StartCoroutine(SpawnCoroutine());
            else
                StartCoroutine(DissolveCoroutine());
        }
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.S))
    //     {
    //         Spawn();
    //     }
    //     if (Input.GetKeyDown(KeyCode.D))
    //     {
    //         Dissolve();
    //     }
    //     
    // }
    
    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(delay);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                PropertyBlock.SetFloat("_VerticalDissolve", t);
                spriteRenderers[i].SetPropertyBlock(PropertyBlock);
            }
            yield return null;
        }

    }
    IEnumerator DissolveCoroutine()
    {
        yield return new WaitForSeconds(delay);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                PropertyBlock.SetFloat("_DissolveAmount", t);
                spriteRenderers[i].SetPropertyBlock(PropertyBlock);
            }
            yield return null;
        }
        Destroy(gameObject);
    }
    
    public void Spawn()
    {
        StartCoroutine(SpawnCoroutine());
    }

    public void Dissolve()
    {
        StartCoroutine(DissolveCoroutine());
    }
}

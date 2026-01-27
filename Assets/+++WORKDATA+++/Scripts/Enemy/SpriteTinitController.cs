using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteTintController : MonoBehaviour
{
    [Header("Tint")]
    [SerializeField] private Color tintColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock mpb;

   
    private static readonly int TintColorID = Shader.PropertyToID("_TintColor");

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        ApplyTint();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        ApplyTint();
    }
#endif

    private void ApplyTint()
    {
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(TintColorID, tintColor);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    
    public void SetTint(Color color)
    {
        tintColor = color;
        ApplyTint();
    }
}



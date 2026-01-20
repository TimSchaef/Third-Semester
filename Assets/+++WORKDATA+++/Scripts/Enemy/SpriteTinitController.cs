using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteTintController : MonoBehaviour
{
    [Header("Tint Settings")]
    [SerializeField] private Color tintColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock mpb;

    // Muss exakt mit der Reference im Shader Graph übereinstimmen
    private static readonly int TintColorID = Shader.PropertyToID("_TintColor");

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        ApplyTint();
    }

#if UNITY_EDITOR
    // Aktualisiert die Farbe sofort, wenn du sie im Inspector änderst
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

    // Optional weiterhin per Code nutzbar
    public void SetTint(Color color)
    {
        tintColor = color;
        ApplyTint();
    }
}



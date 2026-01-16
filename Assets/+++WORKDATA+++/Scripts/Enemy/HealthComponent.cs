using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthComponent : MonoBehaviour
{
    public PlayerStatsManager stats;
    public event Action OnDeath;

    [Header("Fallback HP")]
    [Tooltip("Wird benutzt, wenn kein Stats-Objekt vorhanden ist oder MaxHP aus Stats <= 0 ist.")]
    [SerializeField] private float fallbackMaxHP = 100f;

    [Header("Scaling")]
    [Tooltip("Multiplikator für MaxHP (z.B. für Wellen-Skalierung). 1 = normal.")]
    [SerializeField] private float maxHpMultiplier = 1f;
    public float MaxHpMultiplier => maxHpMultiplier;

    public float CurrentHP { get; private set; }

    public float BaseMaxHP
    {
        get
        {
            float baseFromStats = 0f;
            if (stats != null)
                baseFromStats = stats.GetValue(CoreStatId.MaxHP);

            if (baseFromStats <= 0f)
                baseFromStats = fallbackMaxHP;

            return baseFromStats;
        }
    }

    public float MaxHP => BaseMaxHP * Mathf.Max(0.1f, maxHpMultiplier);

    private bool isDead = false;

    // ---------------------------
    // Damage Flash (Farbwechsel)
    // ---------------------------
    [Header("Damage Flash")]
    [SerializeField] private bool flashOnDamage = true;
    [Tooltip("Farbe beim Treffer (im Inspector auswählbar).")]
    [SerializeField] private Color damageFlashColor = Color.red;

    [Tooltip("Wie lange die Flash-Farbe sofort anliegt, bevor zurückgeblendet wird.")]
    [SerializeField] private float flashHoldTime = 0.05f;

    [Tooltip("Wie lange zurück zur Originalfarbe geblendet wird.")]
    [SerializeField] private float flashReturnTime = 0.10f;

    [Tooltip("Wenn true: nur Player flasht. Wenn false: alle (Player + Gegner).")]
    [SerializeField] private bool flashOnlyPlayer = false;

    [Tooltip("Optional: Renderer manuell setzen. Leer = automatisch Children suchen.")]
    [SerializeField] private Renderer[] renderers3D;

    [Tooltip("Optional: SpriteRenderer manuell setzen. Leer = automatisch Children suchen.")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private Tween flashTween;
    private MaterialPropertyBlock mpb;

    // Shader-Properties: Built-in/Legacy oft _Color, URP oft _BaseColor
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

    private Color[] originalSpriteColors;
    private Color[] original3DColors;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        // Falls nicht im Inspector gesetzt: automatisch sammeln
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers3D == null || renderers3D.Length == 0)
            renderers3D = GetComponentsInChildren<Renderer>(true);

        // Originalfarben merken (2D)
        originalSpriteColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalSpriteColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;

        // Originalfarben merken (3D)
        original3DColors = new Color[renderers3D.Length];
        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null)
            {
                original3DColors[i] = Color.white;
                continue;
            }

            // Default aus Material holen (falls vorhanden)
            Color baseColor = Color.white;
            if (r.sharedMaterial != null)
            {
                if (r.sharedMaterial.HasProperty(BaseColorProp))
                    baseColor = r.sharedMaterial.GetColor(BaseColorProp);
                else if (r.sharedMaterial.HasProperty(ColorProp))
                    baseColor = r.sharedMaterial.GetColor(ColorProp);
            }

            // Wenn PropertyBlock schon Farbe gesetzt hat, nehmen; sonst baseColor
            r.GetPropertyBlock(mpb);
            Color pbColor = Color.clear;
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorProp))
                pbColor = mpb.GetColor(BaseColorProp);
            else
                pbColor = mpb.GetColor(ColorProp);

            original3DColors[i] = (pbColor.a == 0f && baseColor.a != 0f) ? baseColor : pbColor;
        }
    }

    void Start()
    {
        float max = MaxHP;
        if (max <= 0f)
        {
            maxHpMultiplier = 1f;
            max = fallbackMaxHP;
        }

        CurrentHP = max;
    }

    void Update()
    {
        if (isDead) return;

        float regen = 0f;
        if (stats != null)
            regen = Mathf.Max(0f, stats.GetValue(CoreStatId.HPRegen));

        if (regen > 0f && CurrentHP > 0f && CurrentHP < MaxHP)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + regen * Time.deltaTime);
        }
    }

    public void SetMaxHpMultiplier(float mult, bool healToFull = true)
    {
        maxHpMultiplier = Mathf.Max(0.1f, mult);
        if (healToFull)
        {
            CurrentHP = MaxHP;
        }
    }

    public float ApplyDamage(float rawDamage, HealthComponent attacker = null)
    {
        if (isDead || CurrentHP <= 0f) return 0f;

        float armor = 0f;
        if (stats != null)
            armor = Mathf.Max(0f, stats.GetValue(CoreStatId.Armor));

        float reduction = armor / (armor + 100f);
        float taken = Mathf.Max(0f, rawDamage * (1f - reduction));

        if (taken <= 0f) return 0f;

        float previousHP = CurrentHP;
        CurrentHP -= taken;

        // Farb-Flash bei Schaden
        FlashDamage();

        bool diedThisHit = previousHP > 0f && CurrentHP <= 0f;

        float thornsPct = 0f;
        if (stats != null)
            thornsPct = Mathf.Clamp01(stats.GetValue(CoreStatId.Thorns));

        if (thornsPct > 0f && attacker != null && !attacker.isDead && attacker.CurrentHP > 0f)
        {
            float reflect = taken * thornsPct;
            attacker.ApplyPureDamage(reflect);
        }

        if (diedThisHit && attacker != null && attacker.stats != null)
        {
            float lsPct = Mathf.Clamp01(attacker.stats.GetValue(CoreStatId.LifeSteal));
            if (lsPct > 0f)
            {
                float healAmount = taken * lsPct;
                attacker.Heal(healAmount);
            }
        }

        if (CompareTag("Player"))
        {
            if (Camera.main != null)
            {
                Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0f));
            }
        }

        if (diedThisHit)
            Die();

        return taken;
    }

    public void ApplyPureDamage(float amount)
    {
        if (isDead || CurrentHP <= 0f) return;

        CurrentHP -= amount;

        // Farb-Flash bei PureDamage
        FlashDamage();

        if (CurrentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || CurrentHP <= 0f) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        CurrentHP = 0f;

        // Flash abbrechen und Originalfarbe wiederherstellen (optional sauber)
        if (flashTween != null && flashTween.IsActive()) flashTween.Kill();
        RestoreOriginalColors();

        OnDeath?.Invoke();

        if (CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------
    // Flash Implementierung
    // ---------------------------
    private void FlashDamage()
    {
        if (!flashOnDamage) return;
        if (flashOnlyPlayer && !CompareTag("Player")) return;

        // Wenn Flash läuft: abbrechen, original wiederherstellen, dann neu flashen
        if (flashTween != null && flashTween.IsActive())
            flashTween.Kill();

        SetAllToColor(damageFlashColor);

        // Nach kurzer Hold-Time zurückblenden
        flashTween = DOVirtual.DelayedCall(flashHoldTime, () =>
        {
            float t = 0f;
            flashTween = DOTween.To(() => t, x =>
            {
                t = x;
                LerpBackToOriginal(t);
            }, 1f, flashReturnTime);
        });
    }

    private void SetAllToColor(Color c)
    {
        // 2D
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = c;
        }

        // 3D (MaterialPropertyBlock, keine Material-Instanzen)
        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null) continue;

            r.GetPropertyBlock(mpb);

            // Setze beide Properties (URP + Built-in), wenn möglich
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorProp))
                mpb.SetColor(BaseColorProp, c);
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(ColorProp))
                mpb.SetColor(ColorProp, c);

            r.SetPropertyBlock(mpb);
        }
    }

    private void LerpBackToOriginal(float t01)
    {
        // 2D
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            sr.color = Color.Lerp(damageFlashColor, originalSpriteColors[i], t01);
        }

        // 3D
        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null) continue;

            Color target = original3DColors[i];
            Color current = Color.Lerp(damageFlashColor, target, t01);

            r.GetPropertyBlock(mpb);

            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorProp))
                mpb.SetColor(BaseColorProp, current);
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(ColorProp))
                mpb.SetColor(ColorProp, current);

            r.SetPropertyBlock(mpb);
        }
    }

    private void RestoreOriginalColors()
    {
        // 2D
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;
            sr.color = originalSpriteColors[i];
        }

        // 3D
        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null) continue;

            Color target = original3DColors[i];

            r.GetPropertyBlock(mpb);
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorProp))
                mpb.SetColor(BaseColorProp, target);
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(ColorProp))
                mpb.SetColor(ColorProp, target);
            r.SetPropertyBlock(mpb);
        }
    }
}




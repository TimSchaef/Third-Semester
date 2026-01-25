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
    // Damage Numbers
    // ---------------------------
    [Header("Damage Numbers")]
    [Tooltip("Prefab (WorldSpace Canvas + TMP) das die Zahl anzeigt.")]
    [SerializeField] private DamageNumber damageNumberPrefab;

    [Tooltip("Offset über dem Gegner, wo die Zahl erscheinen soll.")]
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("Wenn true: Zahlen nur für Gegner (nicht Player).")]
    [SerializeField] private bool damageNumbersOnlyForEnemies = true;

    // ---------------------------
    // Damage Flash (ShaderGraph Tint)
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

    [Tooltip("Unscaled Time benutzen (empfohlen, wenn du irgendwo timeScale=0 setzt).")]
    [SerializeField] private bool flashIgnoreTimeScale = false;

    [Tooltip("ShaderGraph Tint Reference Name. Muss im ShaderGraph existieren und genutzt werden (Texture RGB * Tint -> BaseColor).")]
    [SerializeField] private string tintPropertyName = "_TintColor";

    [Tooltip("Optional: Renderer manuell setzen. Leer = automatisch Children suchen.")]
    [SerializeField] private Renderer[] renderers3D;

    [Tooltip("Optional: SpriteRenderer manuell setzen. Leer = automatisch Children suchen.")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private Tween flashTween;
    private MaterialPropertyBlock mpb;
    private int tintPropId;

    private Color[] originalSpriteColors;
    private Color[] originalTintColors3D;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        tintPropId = Shader.PropertyToID(tintPropertyName);

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers3D == null || renderers3D.Length == 0)
            renderers3D = GetComponentsInChildren<Renderer>(true);

        originalSpriteColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalSpriteColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;

        originalTintColors3D = new Color[renderers3D.Length];
        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null || r.sharedMaterial == null)
            {
                originalTintColors3D[i] = Color.white;
                continue;
            }

            Color tint = Color.white;

            if (r.sharedMaterial.HasProperty(tintPropId))
                tint = r.sharedMaterial.GetColor(tintPropId);

            r.GetPropertyBlock(mpb);
            Color pb = mpb.GetColor(tintPropId);
            if (pb.a > 0f || pb.r > 0f || pb.g > 0f || pb.b > 0f)
                tint = pb;

            originalTintColors3D[i] = tint;
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
        if (healToFull) CurrentHP = MaxHP;
    }

    // isCrit optional -> kompatibel
    public float ApplyDamage(float rawDamage, HealthComponent attacker = null, bool isCrit = false)
    {
        if (isDead || CurrentHP <= 0f) return 0f;

        // ARMOR ENTFERNT: Schaden 1:1
        float taken = Mathf.Max(0f, rawDamage);
        if (taken <= 0f) return 0f;

        float previousHP = CurrentHP;
        CurrentHP -= taken;

        SpawnDamageNumber(taken, isCrit);
        FlashDamage();
        
        if (CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound3D("playerHit", transform.position);
        }

        
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
                Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0f))
                          .SetUpdate(flashIgnoreTimeScale);
            }
        }

        if (diedThisHit) Die();

        return taken;
    }

    public void ApplyPureDamage(float amount, bool isCrit = false)
    {
        if (isDead || CurrentHP <= 0f) return;

        float taken = Mathf.Max(0f, amount);
        if (taken <= 0f) return;

        CurrentHP -= taken;

        SpawnDamageNumber(taken, isCrit);
        FlashDamage();

        if (CurrentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (isDead || CurrentHP <= 0f) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + Mathf.Max(0f, amount));
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        CurrentHP = 0f;

        if (flashTween != null && flashTween.IsActive()) flashTween.Kill();
        RestoreOriginalColors();

        OnDeath?.Invoke();

        if (CompareTag("Player"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            Destroy(gameObject);
    }

    private void SpawnDamageNumber(float amount, bool isCrit)
    {
        if (!damageNumberPrefab) return;
        if (damageNumbersOnlyForEnemies && CompareTag("Player")) return;

        Vector3 pos = transform.position + damageNumberOffset;
        DamageNumber dn = Instantiate(damageNumberPrefab, pos, Quaternion.identity);
        dn.Init(amount, isCrit);
    }

    private void FlashDamage()
    {
        if (!flashOnDamage) return;
        if (flashOnlyPlayer && !CompareTag("Player")) return;

        if (flashTween != null && flashTween.IsActive())
            flashTween.Kill();

        SetAllToFlashColor(damageFlashColor);

        Tween hold = DOVirtual.DelayedCall(flashHoldTime, () =>
        {
            float t = 0f;
            flashTween = DOTween.To(() => t, x =>
            {
                t = x;
                LerpBackToOriginal(t);
            }, 1f, flashReturnTime).SetUpdate(flashIgnoreTimeScale);
        });

        hold.SetUpdate(flashIgnoreTimeScale);
        flashTween = hold;
    }

    private void SetAllToFlashColor(Color c)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = c;
        }

        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null || r.sharedMaterial == null) continue;
            if (!r.sharedMaterial.HasProperty(tintPropId)) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(tintPropId, c);
            r.SetPropertyBlock(mpb);
        }
    }

    private void LerpBackToOriginal(float t01)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;
            sr.color = Color.Lerp(damageFlashColor, originalSpriteColors[i], t01);
        }

        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null || r.sharedMaterial == null) continue;
            if (!r.sharedMaterial.HasProperty(tintPropId)) continue;

            Color target = originalTintColors3D[i];
            Color current = Color.Lerp(damageFlashColor, target, t01);

            r.GetPropertyBlock(mpb);
            mpb.SetColor(tintPropId, current);
            r.SetPropertyBlock(mpb);
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;
            sr.color = originalSpriteColors[i];
        }

        for (int i = 0; i < renderers3D.Length; i++)
        {
            var r = renderers3D[i];
            if (r == null || r.sharedMaterial == null) continue;
            if (!r.sharedMaterial.HasProperty(tintPropId)) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(tintPropId, originalTintColors3D[i]);
            r.SetPropertyBlock(mpb);
        }
    }
}





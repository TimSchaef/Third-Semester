using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DamageVignetteController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image vignetteImage;

    [Header("Intensity")]
    [SerializeField, Range(0f, 1f)]
    private float maxAlpha = 0.5f;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.08f;
    [SerializeField] private float fadeOutTime = 0.25f;

    [Header("Behavior")]
    [Tooltip("Unscaled Time benutzen (empfohlen).")]
    [SerializeField] private bool ignoreTimeScale = true;

    private Tween currentTween;

    private void Awake()
    {
        if (vignetteImage == null)
            vignetteImage = GetComponent<Image>();

        SetAlpha(0f);
    }
    
    public void Play()
    {
        if (vignetteImage == null)
            return;
        
        currentTween?.Kill();
        
        SetAlpha(0f);
        
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(ignoreTimeScale);

        seq.Append(
            vignetteImage
                .DOFade(maxAlpha, fadeInTime)
                .SetEase(Ease.OutQuad)
        );

        seq.Append(
            vignetteImage
                .DOFade(0f, fadeOutTime)
                .SetEase(Ease.OutQuad)
        );

        currentTween = seq;
    }

    private void SetAlpha(float a)
    {
        Color c = vignetteImage.color;
        c.a = a;
        vignetteImage.color = c;
    }
}


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownIndicator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerAttack attack;   // Referenz zum PlayerAttack
    [SerializeField] private Image fillImage;       // UI Image (Type = Filled)
    [SerializeField] private TMP_Text secondsText;  // optional

    [Header("Fill")]
    [SerializeField] private bool invertFill = false;

    [Header("Text")]
    [SerializeField] private float readyEpsilon = 0.01f;

    private void Reset()
    {
        // Komfort: versucht automatisch ein Filled-Image am selben GameObject zu finden
        fillImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (attack == null || fillImage == null) return;

        // 0..1 von PlayerAttack
        float f = attack.CooldownFill01; // 0 = im Cooldown, 1 = bereit
        float uiFill = invertFill ? (1f - f) : f;
        fillImage.fillAmount = Mathf.Clamp01(uiFill);

        if (secondsText != null)
        {
            float remaining = attack.RemainingCooldown;

            if (remaining <= readyEpsilon)
            {
                secondsText.text = string.Empty;
            }
            else
            {
                secondsText.text = remaining >= 1f
                    ? $"{remaining:0}s"
                    : $"{remaining:0.0}s";
            }
        }
    }
}




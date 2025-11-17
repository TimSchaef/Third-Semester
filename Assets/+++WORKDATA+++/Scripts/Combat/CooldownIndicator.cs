using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownIndicator : MonoBehaviour
{
    [Header("Refs")]
    public PlayerAttack attack;      // Referenz zum PlayerAttack
    public Image fillImage;          // UI Image (Type = Filled)
    public TMP_Text secondsText;     // optional

    [Header("Fill")]
    public bool invertFill = false;  // true = 1->leer / 0->voll, je nach Overlay-Art

    private void Update()
    {
        if (!attack || !fillImage) return;

        // 0..1 von PlayerAttack holen
        float f = attack.CooldownFill01; // 0 = im Cooldown, 1 = bereit
        float uiFill = invertFill ? (1f - f) : f;
        fillImage.fillAmount = Mathf.Clamp01(uiFill);

        if (secondsText)
        {
            float remaining = attack.RemainingCooldown;

            // Wenn bereit -> Text ausblenden
            if (remaining <= 0.01f)
            {
                secondsText.text = "";
            }
            else
            {
                // >1s: ganzzahlige Sekunden, <1s: eine Nachkommastelle
                secondsText.text = remaining >= 1f
                    ? $"{remaining:0}s"
                    : $"{remaining:0.0}s";
            }
        }
    }
}



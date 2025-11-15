using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownIndicator : MonoBehaviour
{
    [Header("Refs")]
    public PlayerAttack attack;  // referenzieren
    public Image fillImage;      // UI Image (Type=Filled)
    public TMP_Text secondsText; // optional

    [Header("Fill")]
    public bool invertFill = false; // true = 1->leer/0->voll, je nach Art des Overlays

    private void Update()
    {
        if (!attack || !fillImage) return;

        float f = attack.CooldownFill01; // 0..1 (1=bereit)
        float uiFill = invertFill ? (1f - f) : f;
        fillImage.fillAmount = Mathf.Clamp01(uiFill);

        if (secondsText)
        {
            // Restzeit anzeigen (ästhetischer bei < 1s zwei Nachkommastellen)
            float remaining = Mathf.Max(0f, attack.CooldownFill01 < 1f ? (1f - attack.CooldownFill01) * attackLastDurationApprox() : 0f);
            secondsText.text = remaining >= 1f ? $"{remaining:0}s" : (remaining > 0f ? $"{remaining:0.0}s" : "");
        }
    }

    // kleine Annäherung der Restzeit, falls du sie magst – basierend auf Fill
    private float attackLastDurationApprox()
    {
        // defensive Annahme: wenn Fill steigt linear von 0->1
        // du kannst auch eine öffentliche Property für lastCooldownDuration herausgeben,
        // falls du exakter sein willst.
        return 1f; // => dann oben lieber "secondsText" deaktivieren oder ersetzen
    }
}


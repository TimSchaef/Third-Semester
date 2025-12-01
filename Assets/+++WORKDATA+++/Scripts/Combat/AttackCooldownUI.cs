using UnityEngine;
using UnityEngine.UI;

public class AttackCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TMPro.TextMeshProUGUI cooldownText; // optional, falls du TMP benutzt

    private void Update()
    {
        if (!playerAttack || !cooldownFillImage) return;

        // Dein PlayerAttack.CooldownFill01:
        // 0 = im Cooldown, 1 = bereit
        float ready01 = playerAttack.CooldownFill01;

        // Variante 1: Leiste füllt sich bis bereit (0 -> 1)
        cooldownFillImage.fillAmount = ready01;

        // Variante 2 (falls du "Rest-Cooldown" darstellen willst):
        // cooldownFillImage.fillAmount = 1f - ready01;

        // Optional: Sekunden anzeigen
        if (cooldownText)
        {
            float remaining = playerAttack.RemainingCooldown;
            cooldownText.text = remaining > 0f 
                ? remaining.ToString("0.0") + "s"
                : "";
        }
    }
}

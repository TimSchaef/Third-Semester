using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthDisplay : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private HealthComponent health;

    [Header("UI")]
    [SerializeField] private TMP_Text hitpointsTextmesh;
    [SerializeField] private Image hpFillImage; // Image Type = Filled

    [Header("Formatting")]
    [SerializeField] private bool showAsIntegers = true;
    [SerializeField] private string textFormat = "{0} / {1}";

    private void Awake()
    {
        if (hitpointsTextmesh == null)
            hitpointsTextmesh = GetComponentInChildren<TMP_Text>(true);

        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                health = player.GetComponent<HealthComponent>();
        }
    }

    private void Update()
    {
        if (health == null || hpFillImage == null) return;

        float max = health.MaxHP;
        float cur = health.CurrentHP;

        float percent = (max <= 0f) ? 0f : Mathf.Clamp01(cur / max);

        // HP-Bar
        hpFillImage.fillAmount = percent;

        // Text
        if (hitpointsTextmesh != null)
        {
            if (showAsIntegers)
            {
                hitpointsTextmesh.text = string.Format(
                    textFormat,
                    Mathf.CeilToInt(cur),
                    Mathf.CeilToInt(max)
                );
            }
            else
            {
                hitpointsTextmesh.text = string.Format(textFormat, cur, max);
            }
        }
    }
}


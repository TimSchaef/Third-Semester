using TMPro;
using UnityEngine;

public class UIHealthText : MonoBehaviour
{
    [SerializeField] private HealthComponent health;      // Player oder Enemy
    [SerializeField] private TMP_Text hitpointsTextmesh;  // TextMeshPro Text

    private void Awake()
    {
        if (hitpointsTextmesh == null)
            hitpointsTextmesh = GetComponentInChildren<TMP_Text>();

        // Falls health nicht gesetzt wurde, versuchen wir den Player zu finden
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                health = player.GetComponent<HealthComponent>();
        }
    }

    private void Update()
    {
        if (!health || !hitpointsTextmesh) return;

        int cur = Mathf.CeilToInt(health.CurrentHP);
        int max = Mathf.CeilToInt(health.MaxHP);

        hitpointsTextmesh.text = $"{cur} / {max}";
    }
}


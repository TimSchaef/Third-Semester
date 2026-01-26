using TMPro;
using UnityEngine;

public class UIHealthText : MonoBehaviour
{
    [SerializeField] private HealthComponent health;      
    [SerializeField] private TMP_Text hitpointsTextmesh;  

    private void Awake()
    {
        if (hitpointsTextmesh == null)
            hitpointsTextmesh = GetComponentInChildren<TMP_Text>();
        
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


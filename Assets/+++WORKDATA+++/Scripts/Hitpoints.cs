using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Hitpoints : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int hitPoints = 100;
    [SerializeField] private int maxHitPoints = 100;          // NEW: for % fill
    [SerializeField] private UIHitPoints uiHitpoints;         // optional: screen-space text
    [SerializeField] private EnemyHealthBar enemyHealthBar;   // NEW: optional world-space bar

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // clamp/initialize
        if (maxHitPoints <= 0) maxHitPoints = hitPoints > 0 ? hitPoints : 1;
        hitPoints = Mathf.Clamp(hitPoints, 0, maxHitPoints);

        // optional UI hookups
        if (uiHitpoints != null)
            uiHitpoints.UpdateHitpoints(hitPoints);

        if (enemyHealthBar != null)
            enemyHealthBar.Init(transform, maxHitPoints, hitPoints);
    }

    /// <summary>
    /// Verursacht Schaden, Knockback und prüft auf Tod/Respawn.
    /// </summary>
    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackForce)
    {
        hitPoints = Mathf.Clamp(hitPoints - Mathf.Max(0, damage), 0, maxHitPoints);

        // Knockback für Rigidbody
        if (rb != null)
        {
            knockbackDirection.y = 0f;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        // Knockback + Stun für Gegner-KI (optional)
        var enemy = GetComponent<CotLStyleEnemy3D>();
        if (enemy != null)
            enemy.Hit(knockbackDirection * knockbackForce, 0.25f);

        // UI aktualisieren (beide sind optional)
        if (uiHitpoints != null)
            uiHitpoints.UpdateHitpoints(hitPoints);

        if (enemyHealthBar != null)
            enemyHealthBar.Set(hitPoints);

        // Wenn HP = 0 → zerstören oder respawnen
        if (hitPoints <= 0)
        {
            if (CompareTag("Player"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("ShakeS");
            Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0));
        }
    }
}



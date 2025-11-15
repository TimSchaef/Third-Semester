using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }

    [Header("Prefab (World-Space Canvas mit DamageNumber)")]
    public DamageNumber damageNumberPrefab;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnDamage(int value, Transform follow, bool isCrit = false)
    {
        if (!damageNumberPrefab || !follow) return;
        var dn = Instantiate(damageNumberPrefab, follow.position, Quaternion.identity);
        var col = isCrit ? new Color(1f, 0.9f, 0.2f) : new Color(1f, 0.25f, 0.2f);
        dn.Init(value.ToString(), col, follow);
        if (isCrit) dn.text.fontSize *= 1.2f;
    }

    public void SpawnHeal(int value, Transform follow)
    {
        if (!damageNumberPrefab || !follow) return;
        var dn = Instantiate(damageNumberPrefab, follow.position, Quaternion.identity);
        dn.Init("+" + value, new Color(0.3f, 1f, 0.3f), follow);
    }

    public void SpawnThorns(int value, Transform follow)
    {
        if (!damageNumberPrefab || !follow) return;
        var dn = Instantiate(damageNumberPrefab, follow.position, Quaternion.identity);
        dn.Init(value.ToString(), new Color(0.6f, 0.9f, 1f), follow);
    }
}


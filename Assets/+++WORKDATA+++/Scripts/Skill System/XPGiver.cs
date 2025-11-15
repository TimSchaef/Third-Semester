using UnityEngine;

public class XPGiver : MonoBehaviour
{
    public PlayerStatsManager stats;
    public PlayerProgress progress;

    public void GrantXP(int baseAmount)
    {
        float mult = Mathf.Max(0f, stats.GetValue(CoreStatId.XPGain)); // 1.0 = normal
        progress.AddXPMultiplied(baseAmount, mult);
    }
}

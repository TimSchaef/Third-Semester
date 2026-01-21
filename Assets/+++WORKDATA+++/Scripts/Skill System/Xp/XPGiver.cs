using UnityEngine;

public class XPGiver : MonoBehaviour
{
    public PlayerStatsManager stats;  // PlayerStatsManager vom Spieler
    public PlayerProgress progress;   // PlayerProgress vom Spieler
    

    public void GrantXP(int baseAmount)
    {
        if (progress == null || stats == null)
        {
            Debug.LogWarning("XPGiver: stats oder progress nicht gesetzt!");
            return;
        }

        // XPGain-Stat als Multiplikator (1.0 = normal, 1.2 = +20% XP, 0.5 = -50% XP)
        float mult = Mathf.Max(0f, stats.GetValue(CoreStatId.XPGain));
        progress.AddXPMultiplied(baseAmount, mult);
    }
}


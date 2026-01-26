using UnityEngine;

public class XPGiver : MonoBehaviour
{
    public PlayerStatsManager stats;  
    public PlayerProgress progress;   
    

    public void GrantXP(int baseAmount)
    {
        if (progress == null || stats == null)
        {
            Debug.LogWarning("XPGiver: stats oder progress nicht gesetzt!");
            return;
        }
        
        float mult = Mathf.Max(0f, stats.GetValue(CoreStatId.XPGain));
        progress.AddXPMultiplied(baseAmount, mult);
        
        SoundManager.Instance.PlaySound3D("xp", transform.position);
    }
}


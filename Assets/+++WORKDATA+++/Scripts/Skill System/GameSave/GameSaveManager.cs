using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public PlayerProgress progress;
    public PlayerStatsManager stats;
    public SkillTree skillTree;

    void Awake()
    {
        if (progress != null)
            progress.LoadProgress();

        if (stats != null)
            stats.LoadStats();

        if (skillTree != null)
            skillTree.LoadUnlocked();
    }

    public void SaveAll()
    {
        if (progress != null)
            progress.SaveProgress();

        if (stats != null)
            stats.SaveStats();

        if (skillTree != null)
            skillTree.SaveUnlocked();
    }

    void OnApplicationQuit()
    {
        SaveAll();
    }
}

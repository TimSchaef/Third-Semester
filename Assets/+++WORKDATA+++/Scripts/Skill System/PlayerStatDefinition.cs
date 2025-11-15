using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Player Stat (Fixed Set)", fileName = "NewPlayerStat")]
public class PlayerStatDefinition : ScriptableObject
{
    public CoreStatId statId;
    public string displayName;
    [TextArea] public string description;

    [Header("Upgrade")]
    [Min(1)] public int maxLevel = 5;
    [Min(1)] public int costPerUpgrade = 1;
    [Min(1)] public int requiredPlayerLevel = 1;
    public List<PlayerStatDefinition> prerequisites = new List<PlayerStatDefinition>();

    [Header("Values")]
    public float baseValue = 0f;
    public float incrementPerLevel = 0f;  // wird pro Upgrade addiert

    public float GetValueAtLevel(int level)
    {
        level = Mathf.Max(0, level);
        return baseValue + Mathf.Max(0, level - 1) * incrementPerLevel;
    }
}


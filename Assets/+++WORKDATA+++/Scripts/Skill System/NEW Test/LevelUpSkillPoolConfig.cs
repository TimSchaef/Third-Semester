using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Level Up/Skill Pool Config", fileName = "LevelUpSkillPoolConfig")]
public class LevelUpSkillPoolConfig : ScriptableObject
{
    [System.Serializable]
    public class PoolEntry
    {
        [Header("Which levels?")]
        [Tooltip("If not empty: This pool is used only for these exact levels (e.g., 3, 15, 20).")]
        public List<int> exactLevels = new List<int>();

        [Tooltip("If exactLevels is empty: pool is used when level is within [minLevel..maxLevel].")]
        public int minLevel = 1;

        public int maxLevel = 999;

        [Header("Pool Skills")]
        public List<SkillDefinition> skills = new List<SkillDefinition>();

        [Header("Behavior")]
        [Tooltip("If true: Can also include Skills from the global SkillTree list (as fallback) when pool is too small.")]
        public bool allowFallbackToGlobalPool = false;
    }

    [Header("Pools (top to bottom priority)")]
    public List<PoolEntry> pools = new List<PoolEntry>();

    /// <summary>
    /// Returns the first matching pool (priority = list order).
    /// </summary>
    public bool TryGetPoolForLevel(int level, out List<SkillDefinition> skills, out bool allowFallbackToGlobalPool)
    {
        skills = null;
        allowFallbackToGlobalPool = false;

        foreach (var p in pools)
        {
            if (p == null) continue;

            bool match =
                (p.exactLevels != null && p.exactLevels.Count > 0 && p.exactLevels.Contains(level))
                || ((p.exactLevels == null || p.exactLevels.Count == 0) && level >= p.minLevel && level <= p.maxLevel);

            if (!match) continue;

            skills = p.skills != null ? p.skills.Where(s => s != null).Distinct().ToList() : new List<SkillDefinition>();
            allowFallbackToGlobalPool = p.allowFallbackToGlobalPool;
            return true;
        }

        return false;
    }
}


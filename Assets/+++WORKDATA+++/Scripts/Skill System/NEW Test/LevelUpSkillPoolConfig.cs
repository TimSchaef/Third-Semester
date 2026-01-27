using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Level Up/Skill Pool Config", fileName = "LevelUpSkillPoolConfig")]
public class LevelUpSkillPoolConfig : ScriptableObject
{
    [System.Serializable]
    public class PoolEntry
    {
        [Header("levels")]
        public List<int> exactLevels = new List<int>();
        
        public int minLevel = 1;

        public int maxLevel = 999;

        [Header("Pool Skills")]
        public List<SkillDefinition> skills = new List<SkillDefinition>();

        [Header("Behavior")]
        public bool allowFallbackToGlobalPool = false;
    }

    [Header("Pools")]
    public List<PoolEntry> pools = new List<PoolEntry>();

    
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


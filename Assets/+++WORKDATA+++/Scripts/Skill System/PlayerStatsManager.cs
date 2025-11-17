using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatOp { Add, Mult }

[System.Serializable]
public struct SkillEffect
{
    public CoreStatId stat;
    public StatOp op;
    public float value;
}

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerProgress player;
    public List<PlayerStatDefinition> allStats;

    private readonly Dictionary<CoreStatId, int> levels = new();
    private readonly Dictionary<string, List<SkillEffect>> activeSourceEffects = new();

    // --- SAVE SUPPORT TYPES ---
    [System.Serializable]
    public class StatLevelSave
    {
        public CoreStatId id;
        public int level;
    }

    [System.Serializable]
    public class StatLevelSaveContainer
    {
        public List<StatLevelSave> stats = new();
    }

    void Awake()
    {
        foreach (var s in allStats)
            if (!levels.ContainsKey(s.statId))
                levels.Add(s.statId, 0);
    }

    public int GetLevel(CoreStatId id) =>
        levels.TryGetValue(id, out var lv) ? lv : 0;

    private PlayerStatDefinition GetDef(CoreStatId id) =>
        allStats.FirstOrDefault(s => s.statId == id);

    private float GetBaseValue(CoreStatId id)
    {
        var def = GetDef(id);
        if (!def) return 0f;
        int lv = GetLevel(id);
        return lv > 0 ? def.GetValueAtLevel(lv) : def.baseValue;
    }

    // --- Modifikatoren ---
    public void ApplyEffectsFrom(string sourceId, IEnumerable<SkillEffect> effects)
    {
        if (string.IsNullOrEmpty(sourceId)) return;
        activeSourceEffects[sourceId] = effects?.ToList() ?? new List<SkillEffect>();
    }

    public void RemoveEffectsFrom(string sourceId)
    {
        if (string.IsNullOrEmpty(sourceId)) return;
        activeSourceEffects.Remove(sourceId);
    }

    public float GetValue(CoreStatId id)
    {
        float baseVal = GetBaseValue(id);
        float add = 0f;
        float mult = 0f;

        foreach (var src in activeSourceEffects.Values)
        {
            foreach (var e in src)
            {
                if (e.stat != id) continue;
                if (e.op == StatOp.Add) add += e.value;
                else mult += e.value;
            }
        }

        return (baseVal + add) * (1f + mult);
    }

    // --- Saving Stat Levels ---
    const string KEY_STAT_LEVELS = "player_stat_levels";

    public void SaveStats()
    {
        var container = new StatLevelSaveContainer();

        foreach (var kv in levels)
            container.stats.Add(new StatLevelSave { id = kv.Key, level = kv.Value });

        string json = JsonUtility.ToJson(container);
        PlayerPrefs.SetString(KEY_STAT_LEVELS, json);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        if (!PlayerPrefs.HasKey(KEY_STAT_LEVELS)) return;

        string json = PlayerPrefs.GetString(KEY_STAT_LEVELS, "");
        var container = JsonUtility.FromJson<StatLevelSaveContainer>(json);
        if (container == null) return;

        foreach (var entry in container.stats)
            levels[entry.id] = entry.level;
    }

    // --- Upgrade logic (unverändert) ---
    public bool CanUpgrade(PlayerStatDefinition s, out string reason)
    {
        reason = "";
        int lv = GetLevel(s.statId);

        if (lv >= s.maxLevel) { reason = "Max Level erreicht."; return false; }
        if (player.Level < s.requiredPlayerLevel) { reason = $"Benötigt Spielerlevel {s.requiredPlayerLevel}."; return false; }
        if (s.prerequisites.Any(p => GetLevel(p.statId) == 0)) { reason = "Voraussetzung fehlt."; return false; }
        if (player.SkillPoints < s.costPerUpgrade) { reason = "Nicht genug Skillpunkte."; return false; }

        return true;
    }

    public bool TryUpgrade(PlayerStatDefinition s)
    {
        if (!CanUpgrade(s, out _)) return false;
        if (!player.SpendSkillPoints(s.costPerUpgrade)) return false;

        levels[s.statId] = GetLevel(s.statId) + 1;
        return true;
    }
}




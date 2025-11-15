using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerProgress player;
    public List<PlayerStatDefinition> allStats;

    private readonly Dictionary<CoreStatId, int> levels = new();

    void Awake()
    {
        foreach (var s in allStats)
            if (!levels.ContainsKey(s.statId)) levels.Add(s.statId, 0);
    }

    public int GetLevel(CoreStatId id) => levels.TryGetValue(id, out var lv) ? lv : 0;

    public float GetValue(CoreStatId id)
    {
        var def = allStats.FirstOrDefault(s => s.statId == id);
        if (!def) return 0f;
        int lv = GetLevel(id);
        return lv > 0 ? def.GetValueAtLevel(lv) : def.baseValue;
    }

    bool MeetsPrereq(PlayerStatDefinition s) =>
        s.prerequisites == null || s.prerequisites.All(p => GetLevel(p.statId) > 0);

    public bool CanUpgrade(PlayerStatDefinition s, out string reason)
    {
        reason = "";
        int lv = GetLevel(s.statId);
        if (lv >= s.maxLevel) { reason = "Max Level erreicht."; return false; }
        if (player.Level < s.requiredPlayerLevel) { reason = $"Benötigt Spielerlevel {s.requiredPlayerLevel}."; return false; }
        if (!MeetsPrereq(s)) { reason = "Voraussetzungen fehlen."; return false; }
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


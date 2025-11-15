using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatOp { Add, Mult } // Mult = prozentual (z. B. +10% = 0.10f)

[System.Serializable]
public struct SkillEffect
{
    public CoreStatId stat;
    public StatOp op;
    public float value; // Add: absolute Punkte, Mult: 0.10 = +10%
}

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerProgress player;
    public List<PlayerStatDefinition> allStats;

    private readonly Dictionary<CoreStatId, int> levels = new(); // aus Upgrades
    // NEW: aktive Modifikatoren gruppiert nach Quelle (nodeId)
    private readonly Dictionary<string, List<SkillEffect>> activeSourceEffects = new();

    void Awake()
    {
        foreach (var s in allStats)
            if (!levels.ContainsKey(s.statId)) levels.Add(s.statId, 0);
    }

    // --- Level/Upgrades API (wie gehabt)
    public int GetLevel(CoreStatId id) => levels.TryGetValue(id, out var lv) ? lv : 0;

    private PlayerStatDefinition GetDef(CoreStatId id) => allStats.FirstOrDefault(s => s.statId == id);

    private float GetBaseValue(CoreStatId id)
    {
        var def = GetDef(id);
        if (!def) return 0f;
        int lv = GetLevel(id);
        return lv > 0 ? def.GetValueAtLevel(lv) : def.baseValue;
    }

    // --- Modifikatoren API
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

    // --- Effektiver Wert: Basis + Add + Mult
    public float GetValue(CoreStatId id)
    {
        float baseVal = GetBaseValue(id);

        float add = 0f;
        float mult = 0f; // Summe der Prozente (0.1 + 0.2 = +30%)

        foreach (var kv in activeSourceEffects)
        {
            foreach (var e in kv.Value)
            {
                if (e.stat != id) continue;
                if (e.op == StatOp.Add) add += e.value;
                else if (e.op == StatOp.Mult) mult += e.value;
            }
        }

        return (baseVal + add) * (1f + mult);
    }

    // --- Upgrade-Checks (optional wie zuvor)
    public bool CanUpgrade(PlayerStatDefinition s, out string reason)
    {
        reason = "";
        int lv = GetLevel(s.statId);
        if (lv >= s.maxLevel) { reason = "Max Level erreicht."; return false; }
        if (player.Level < s.requiredPlayerLevel) { reason = $"Benötigt Spielerlevel {s.requiredPlayerLevel}."; return false; }
        if (s.prerequisites != null && s.prerequisites.Any(p => GetLevel(p.statId) == 0)) { reason = "Voraussetzungen fehlen."; return false; }
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



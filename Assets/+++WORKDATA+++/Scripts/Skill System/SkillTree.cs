using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Config")]
    public PlayerStatsManager statsManager;
    public PlayerProgress player;
    public List<SkillDefinition> allSkills = new();

    [Header("Runtime State")]
    [SerializeField] private List<SkillDefinition> unlockedSkills = new();
    private HashSet<string> unlockedIds = new();

    public event Action<SkillDefinition> OnSkillUnlocked;

    void Awake()
    {
        unlockedSkills.Clear();
        unlockedIds.Clear();
        
        ReapplyAllEffects();
    }
    
    public bool IsUnlocked(SkillDefinition skill) =>
        skill != null && unlockedIds.Contains(skill.skillId);
    
    public bool CanUnlock(SkillDefinition skill, out string reason)
    {
        reason = "";

        if (skill == null)
        {
            reason = "No skill.";
            return false;
        }

        if (player == null)
        {
            reason = "No PlayerProgress reference.";
            return false;
        }

        if (player.Level < skill.requiredPlayerLevel)
        {
            reason = $"Requires player level {skill.requiredPlayerLevel}.";
            return false;
        }

        if (player.SkillPoints < skill.costSkillPoints)
        {
            reason = $"Requires {skill.costSkillPoints} skill point(s).";
            return false;
        }

        return true;
    }
    
    public bool TryUnlock(SkillDefinition skill)
    {
        if (skill == null) return false;

        string reason;
        if (!CanUnlock(skill, out reason))
        {
            Debug.Log($"[SkillTree] Cannot unlock {skill.skillId}: {reason} " +
                      $"(Level={player?.Level}, SkillPoints={player?.SkillPoints})");
            return false;
        }

        if (!player.SpendSkillPoints(skill.costSkillPoints))
        {
            Debug.Log($"[SkillTree] SpendSkillPoints failed for {skill.skillId}");
            return false;
        }
        
        if (!unlockedIds.Contains(skill.skillId))
        {
            unlockedSkills.Add(skill);
            unlockedIds.Add(skill.skillId);
        }

        ApplySkillEffects(skill);

        OnSkillUnlocked?.Invoke(skill);

        Debug.Log($"[SkillTree] Unlocked {skill.skillId}. Remaining SkillPoints={player.SkillPoints}");
        return true;
    }

    // ---------- EFFEKTE ----------

    void ApplySkillEffects(SkillDefinition skill)
    {
        if (statsManager == null || skill == null) return;
        statsManager.ApplyEffectsFrom(skill.skillId, skill.effects);
    }

    public void ReapplyAllEffects()
    {
        if (statsManager == null) return;

        // erst alle alten Effekte raus
        foreach (var s in allSkills)
        {
            if (s != null && !string.IsNullOrEmpty(s.skillId))
                statsManager.RemoveEffectsFrom(s.skillId);
        }

        // dann alle freigeschalteten (nur für DIESE RUNDE) wieder drauf
        foreach (var s in unlockedSkills)
        {
            ApplySkillEffects(s);
        }
    }
}







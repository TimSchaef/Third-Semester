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

    const string KEY_UNLOCKED = "skilltree_unlocked";

    void Awake()
    {
        // Geladene Skills wiederherstellen
        LoadUnlocked();

        // HashSet neu aufbauen
        unlockedIds = new HashSet<string>(unlockedSkills.Where(s => s != null).Select(s => s.skillId));

        // Effekte aller bereits freigeschalteten Skills anwenden
        ReapplyAllEffects();
    }

    public bool IsUnlocked(SkillDefinition skill) =>
        skill != null && unlockedIds.Contains(skill.skillId);

    // Prerequisites direkt aus dem SkillDefinition-Asset
    public bool MeetsPrerequisites(SkillDefinition skill)
    {
        if (skill == null) return false;
        if (skill.prerequisites == null || skill.prerequisites.Count == 0)
            return true;

        return skill.prerequisites.All(p => p != null && IsUnlocked(p));
    }

    public bool CanUnlock(SkillDefinition skill, out string reason)
    {
        reason = "";

        if (skill == null)
        {
            reason = "No skill.";
            return false;
        }

        if (IsUnlocked(skill))
        {
            reason = "Already unlocked.";
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

        if (!MeetsPrerequisites(skill))
        {
            reason = "Missing prerequisite.";
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

        unlockedSkills.Add(skill);
        unlockedIds.Add(skill.skillId);

        ApplySkillEffects(skill);
        SaveUnlocked();

        OnSkillUnlocked?.Invoke(skill);

        Debug.Log($"[SkillTree] Unlocked {skill.skillId}. Remaining SkillPoints={player.SkillPoints}");
        return true;
    }

    // ---------- SAVE / LOAD ----------

    public void SaveUnlocked()
    {
        string data = string.Join("|", unlockedIds);
        PlayerPrefs.SetString(KEY_UNLOCKED, data);
        PlayerPrefs.Save();
    }

    public void LoadUnlocked()
    {
        unlockedSkills.Clear();
        unlockedIds.Clear();

        string data = PlayerPrefs.GetString(KEY_UNLOCKED, "");
        if (string.IsNullOrEmpty(data)) return;

        var ids = data.Split('|');
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id)) continue;
            var sk = allSkills.FirstOrDefault(s => s != null && s.skillId == id);
            if (sk != null)
            {
                unlockedSkills.Add(sk);
                unlockedIds.Add(id);
            }
        }
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

        // Erst alte Effekte weg (falls du RemoveEffectsFrom nutzt)
        foreach (var s in allSkills)
        {
            if (s != null && !string.IsNullOrEmpty(s.skillId))
                statsManager.RemoveEffectsFrom(s.skillId);
        }

        // Dann alle unlocked Skills erneut anwenden
        foreach (var s in unlockedSkills)
        {
            ApplySkillEffects(s);
        }
    }
}





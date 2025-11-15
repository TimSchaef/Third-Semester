using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Config")]
    public PlayerProgress player;
    public List<SkillDefinition> allSkills = new List<SkillDefinition>();

    [Header("Runtime State")]
    [SerializeField] private List<SkillDefinition> unlockedSkills = new List<SkillDefinition>();
    private HashSet<string> unlockedIds = new HashSet<string>();

    public event Action<SkillDefinition> OnSkillUnlocked;

    void Awake()
    {
        // Rebuild set on load/enter play
        unlockedIds = new HashSet<string>(unlockedSkills.Select(s => s.skillId));
    }

    public bool IsUnlocked(SkillDefinition skill) => skill != null && unlockedIds.Contains(skill.skillId);

    public bool MeetsPrerequisites(SkillDefinition skill)
    {
        if (skill == null) return false;
        if (skill.prerequisites == null || skill.prerequisites.Count == 0) return true;
        return skill.prerequisites.All(p => p != null && IsUnlocked(p));
    }

    public bool CanUnlock(SkillDefinition skill, out string reason)
    {
        reason = string.Empty;
        if (skill == null) { reason = "No skill selected."; return false; }
        if (IsUnlocked(skill)) { reason = "Already unlocked."; return false; }
        if (player.Level < skill.requiredPlayerLevel)
        {
            reason = $"Requires player level {skill.requiredPlayerLevel}.";
            return false;
        }
        if (!MeetsPrerequisites(skill))
        {
            reason = "Missing prerequisite skill(s).";
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
        if (!CanUnlock(skill, out _)) return false;
        if (!player.SpendSkillPoints(skill.costSkillPoints)) return false;

        unlockedSkills.Add(skill);
        unlockedIds.Add(skill.skillId);
        OnSkillUnlocked?.Invoke(skill);
        SaveUnlocked();
        return true;
    }

    // --- Simple persistence for unlocked set
    const string KEY_UNLOCKED = "skilltree_unlocked";

    public void SaveUnlocked()
    {
        // Store IDs joined by '|'
        var s = string.Join("|", unlockedIds);
        PlayerPrefs.SetString(KEY_UNLOCKED, s);
        PlayerPrefs.Save();
    }

    public void LoadUnlocked()
    {
        unlockedSkills.Clear();
        unlockedIds.Clear();

        var s = PlayerPrefs.GetString(KEY_UNLOCKED, "");
        if (string.IsNullOrEmpty(s)) return;

        var idSet = new HashSet<string>(s.Split('|').Where(id => !string.IsNullOrEmpty(id)));
        foreach (var def in allSkills)
        {
            if (idSet.Contains(def.skillId))
            {
                unlockedSkills.Add(def);
                unlockedIds.Add(def.skillId);
            }
        }
    }
}


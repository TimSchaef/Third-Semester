using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Config")]
    public PlayerProgress player;
    public PlayerStatsManager statsManager;
    public List<SkillDefinition> allSkills = new List<SkillDefinition>();

    [Header("Runtime State")]
    [SerializeField] private List<SkillDefinition> unlockedSkills = new List<SkillDefinition>();
    private HashSet<string> unlockedIds = new HashSet<string>();

    public event Action<SkillDefinition> OnSkillUnlocked;

    void Awake()
    {
        unlockedIds = new HashSet<string>(unlockedSkills.Select(s => s.skillId));
        ReapplyAllEffects();
    }

    public bool IsUnlocked(SkillDefinition skill) =>
        skill != null && unlockedIds.Contains(skill.skillId);

    /// <summary>
    /// Prüft, ob die Voraussetzungen eines Skills erfüllt sind.
    /// Mode:
    ///  - All: alle in prerequisites müssen unlocked sein
    ///  - Any: mindestens eine/r in prerequisites muss unlocked sein
    /// </summary>
    public bool MeetsPrerequisites(SkillDefinition skill)
    {
        if (skill == null) return false;
        if (skill.prerequisites == null || skill.prerequisites.Count == 0)
            return true;

        var validPrereqs = skill.prerequisites.Where(p => p != null).ToList();
        if (validPrereqs.Count == 0) return true;

        switch (skill.prerequisiteMode)
        {
            case SkillPrerequisiteMode.All:
                return validPrereqs.All(p => IsUnlocked(p));

            case SkillPrerequisiteMode.Any:
                return validPrereqs.Any(p => IsUnlocked(p));

            default:
                return true;
        }
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
            if (skill.prerequisites != null && skill.prerequisites.Count > 0)
            {
                if (skill.prerequisiteMode == SkillPrerequisiteMode.All)
                    reason = "Requires all prerequisite skills.";
                else
                    reason = "Requires at least one prerequisite skill.";
            }
            else
            {
                reason = "Missing prerequisite skill(s).";
            }
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

        if (statsManager != null && skill.effects != null)
            statsManager.ApplyEffectsFrom(skill.skillId, skill.effects);

        OnSkillUnlocked?.Invoke(skill);
        SaveUnlocked();
        return true;
    }

    const string KEY_UNLOCKED = "skilltree_unlocked";

    public void SaveUnlocked()
    {
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

        ReapplyAllEffects();
    }

    private void ReapplyAllEffects()
    {
        if (statsManager == null) return;

        foreach (var skill in allSkills)
        {
            if (!string.IsNullOrEmpty(skill.skillId))
                statsManager.RemoveEffectsFrom(skill.skillId);
        }

        foreach (var skill in unlockedSkills)
        {
            if (skill != null && skill.effects != null)
                statsManager.ApplyEffectsFrom(skill.skillId, skill.effects);
        }
    }
}



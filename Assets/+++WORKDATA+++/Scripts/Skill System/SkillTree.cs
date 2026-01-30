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

    
    private readonly Dictionary<string, int> pickCounts = new();

   
    private readonly List<string> appliedSourceIds = new();

    public event Action<SkillDefinition> OnSkillUnlocked;

    void Awake()
    {
        pickCounts.Clear();
        appliedSourceIds.Clear();
        ReapplyAllEffects();
    }

    public int GetPickCount(SkillDefinition skill)
    {
        if (skill == null || string.IsNullOrEmpty(skill.skillId)) return 0;
        return pickCounts.TryGetValue(skill.skillId, out var c) ? c : 0;
    }

    private bool PrerequisitesMet(SkillDefinition skill)
    {
        if (skill == null) return false;
        if (skill.prerequisites == null || skill.prerequisites.Count == 0) return true;

        bool PickedAtLeastOnce(SkillDefinition s) => s != null && GetPickCount(s) > 0;

        var prereqs = skill.prerequisites.Where(p => p != null).ToList();
        if (prereqs.Count == 0) return true;

        return skill.prerequisiteMode switch
        {
            SkillPrerequisiteMode.All => prereqs.All(PickedAtLeastOnce),
            SkillPrerequisiteMode.Any => prereqs.Any(PickedAtLeastOnce),
            _ => true
        };
    }

    public bool CanUnlock(SkillDefinition skill, out string reason)
    {
        reason = "";

        if (skill == null) { reason = "No skill."; return false; }
        if (player == null) { reason = "No PlayerProgress reference."; return false; }
        if (string.IsNullOrEmpty(skill.skillId)) { reason = "Skill has no skillId."; return false; }

        
        int alreadyPicked = GetPickCount(skill);
        if (skill.maxPicks > 0 && alreadyPicked >= skill.maxPicks)
        {
            reason = $"Max picks reached ({skill.maxPicks}).";
            return false;
        }

        
        if (skill.dropWeight <= 0f)
        {
            reason = "Drop weight is 0.";
            return false;
        }

        if (!PrerequisitesMet(skill))
        {
            reason = skill.prerequisiteMode == SkillPrerequisiteMode.All
                ? "Requires all prerequisite skills."
                : "Requires at least one prerequisite skill.";
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

        if (!CanUnlock(skill, out var reason))
        {
            return false;
        }

        if (!player.SpendSkillPoints(skill.costSkillPoints))
        {
            return false;
        }

        int newCount = GetPickCount(skill) + 1;
        pickCounts[skill.skillId] = newCount;

        
        string sourceId = $"{skill.skillId}#{newCount}";
        ApplySkillEffects(sourceId, skill);

        OnSkillUnlocked?.Invoke(skill);
        return true;
    }

    void ApplySkillEffects(string sourceId, SkillDefinition skill)
    {
        if (statsManager == null || skill == null) return;
        statsManager.ApplyEffectsFrom(sourceId, skill.effects);
        appliedSourceIds.Add(sourceId);
    }

    public void ReapplyAllEffects()
    {
        if (statsManager == null) return;

        foreach (var id in appliedSourceIds)
            statsManager.RemoveEffectsFrom(id);

        appliedSourceIds.Clear();

        foreach (var skill in allSkills)
        {
            if (skill == null || string.IsNullOrEmpty(skill.skillId)) continue;

            int count = GetPickCount(skill);
            for (int i = 1; i <= count; i++)
            {
                string sourceId = $"{skill.skillId}#{i}";
                ApplySkillEffects(sourceId, skill);
            }
        }
    }

   

    public List<SkillDefinition> GetRandomUnlockableSkills(int count)
    {
        return GetRandomUnlockableSkillsWeightedFrom(allSkills, count, allowFallbackToGlobal: false);
    }


    public List<SkillDefinition> GetRandomUnlockableSkillsWeightedFrom(
        IList<SkillDefinition> pool,
        int count,
        bool allowFallbackToGlobal)
    {
        count = Mathf.Max(0, count);
        if (count == 0) return new List<SkillDefinition>();

        var basePool = (pool ?? Array.Empty<SkillDefinition>())
            .Where(s => s != null)
            .Distinct()
            .ToList();

        
        var candidates = basePool
            .Where(s => CanUnlock(s, out _))
            .ToList();

        
        if (allowFallbackToGlobal && candidates.Count < count)
        {
            var globalExtra = allSkills
                .Where(s => s != null)
                .Distinct()
                .Where(s => !basePool.Contains(s))
                .Where(s => CanUnlock(s, out _))
                .ToList();

            candidates.AddRange(globalExtra);
        }

        
        var result = new List<SkillDefinition>(count);
        count = Mathf.Min(count, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            var picked = WeightedPick(candidates);
            if (picked == null) break;

            result.Add(picked);
            candidates.Remove(picked);
        }

        return result;
    }

    private SkillDefinition WeightedPick(List<SkillDefinition> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        float total = 0f;
        for (int i = 0; i < pool.Count; i++)
            total += Mathf.Max(0f, pool[i].dropWeight);

        if (total <= 0f) return null;

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            acc += Mathf.Max(0f, pool[i].dropWeight);
            if (r <= acc)
                return pool[i];
        }

        return pool[pool.Count - 1];
    }
}










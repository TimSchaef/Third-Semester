using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Config")]
    public PlayerStatsManager statsManager;   // PlayerStatsManager auf dem Player
    public PlayerProgress player;            // PlayerProgress auf dem Player
    public List<SkillDefinition> allSkills = new(); // alle Skill-Assets (einmal pro Asset eintragen)

    [Header("Runtime State")]
    [SerializeField] private List<SkillDefinition> unlockedSkills = new(); // nur zum Debug/Inspector
    private HashSet<string> unlockedIds = new();                           // für schnellen Zugriff

    public event Action<SkillDefinition> OnSkillUnlocked;

    // eigener Key, damit alte Saves nicht stören
    const string KEY_UNLOCKED = "skilltree_unlocked_v2";

    void Awake()
    {
        LoadUnlocked();

        unlockedIds = new HashSet<string>(
            unlockedSkills.Where(s => s != null).Select(s => s.skillId)
        );

        ReapplyAllEffects();
    }

    /// <summary>
    /// Wird nur noch von Dingen benutzt, die wissen wollen,
    /// ob dieses Skill-Asset schon irgendwann freigeschaltet wurde.
    /// Die Baumlogik (welcher Button nach welchem kommt) läuft über SkillNodeButton.
    /// </summary>
    public bool IsUnlocked(SkillDefinition skill) =>
        skill != null && unlockedIds.Contains(skill.skillId);

    /// <summary>
    /// Prüft NUR:
    /// - Skill != null
    /// - PlayerProgress vorhanden
    /// - Spielerlevel reicht
    /// - genug Skillpunkte
    /// KEINE Button-Verknüpfungen, KEINE SkillDefinition-Prerequisites.
    /// </summary>
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

    /// <summary>
    /// Zieht Skillpunkte ab, markiert das Skill-Asset als freigeschaltet,
    /// wendet Effekte an und speichert.
    /// Die Entscheidung "darf ich von diesem Button aus skillen"
    /// trifft der SkillNodeButton (über seine prerequisiteNodes).
    /// </summary>
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

        foreach (var id in data.Split('|'))
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

        // erst alle alten Effekte raus
        foreach (var s in allSkills)
        {
            if (s != null && !string.IsNullOrEmpty(s.skillId))
                statsManager.RemoveEffectsFrom(s.skillId);
        }

        // dann alle freigeschalteten wieder drauf
        foreach (var s in unlockedSkills)
        {
            ApplySkillEffects(s);
        }
    }
}






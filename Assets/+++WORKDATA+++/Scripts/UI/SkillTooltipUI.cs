using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class SkillTooltipUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text statsText;

    [Header("Refs")]
    [SerializeField] private PlayerStatsManager statsManager;

    [Header("Default")]
    [SerializeField] private string defaultTitle = "Choose a titel";
    [SerializeField] private string defaultStats = "";

    private const string PREVIEW_SOURCE_ID = "skill_preview";

    public void SetDefault()
    {
        ClearPreview();

        if (titleText) titleText.text = defaultTitle;
        if (infoText) infoText.text = ""; 
        if (statsText) statsText.text = defaultStats;
    }

    public void ShowSkill(SkillDefinition skill, string infoTextFromSkill)
    {
        ClearPreview();

        if (skill == null)
        {
            SetDefault();
            return;
        }

        if (titleText) titleText.text = skill.displayName;
        if (infoText) infoText.text = infoTextFromSkill ?? "";
        if (statsText) statsText.text = BuildStatsPreview(skill);
    }

    private string BuildStatsPreview(SkillDefinition skill)
    {
        if (statsManager == null || skill.effects == null || skill.effects.Count == 0)
            return "";

        var affectedStats = skill.effects.Select(e => e.stat).Distinct().ToList();

        var before = new Dictionary<CoreStatId, float>();
        foreach (var stat in affectedStats)
            before[stat] = statsManager.GetValue(stat);

        statsManager.ApplyEffectsFrom(PREVIEW_SOURCE_ID, skill.effects);

        var after = new Dictionary<CoreStatId, float>();
        foreach (var stat in affectedStats)
            after[stat] = statsManager.GetValue(stat);

        ClearPreview();

        var sb = new StringBuilder();
        foreach (var stat in affectedStats)
        {
            float b = before[stat];
            float a = after[stat];

            if (Mathf.Approximately(a, b))
                continue;

            sb.Append(StatTextUtil.GetDisplayName(stat))
              .Append(": ")
              .Append(StatTextUtil.FormatValue(stat, b))
              .Append(" -> ")
              .Append(StatTextUtil.FormatValue(stat, a))
              .AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private void ClearPreview()
    {
        if (statsManager != null)
            statsManager.RemoveEffectsFrom(PREVIEW_SOURCE_ID);
    }
}





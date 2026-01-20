using System;
using TMPro;
using UnityEngine;

public class StatPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform statsContent;   // Content der ScrollView
    [SerializeField] private StatRowUI statRowPrefab;  // Prefab für eine Zeile

    [Header("References")]
    [SerializeField] private PlayerStatsManager statsManager; // liest Werte über GetValue(...)
    [SerializeField] private PlayerProgress progress;         // optional (Level/XP etc.)

    [Header("Options")]
    [SerializeField] private bool rebuildOnEnable = true;
    [SerializeField] private bool showProgressRows = true;

    private void OnEnable()
    {
        if (rebuildOnEnable)
            Refresh();
    }

    /// <summary>
    /// Baut die Stats-Liste komplett neu.
    /// Kannst du aufrufen, wenn sich Werte ändern (z.B. nach Items/LevelUp).
    /// </summary>
    public void Refresh()
    {
        if (!statsContent || !statRowPrefab || statsManager == null)
            return;

        ClearRows();

        if (showProgressRows && progress != null)
        {
            AddRow("Level", progress.Level.ToString());
            AddRow("Skill Points", progress.SkillPoints.ToString());
        }

        foreach (CoreStatId id in Enum.GetValues(typeof(CoreStatId)))
        {
            float v = statsManager.GetValue(id);
            AddRow(id.ToString(), FormatStat(id, v));
        }
    }

    private void ClearRows()
    {
        for (int i = statsContent.childCount - 1; i >= 0; i--)
            Destroy(statsContent.GetChild(i).gameObject);
    }

    private void AddRow(string name, string value)
    {
        var row = Instantiate(statRowPrefab, statsContent);
        row.Set(name, value);
    }

    private string FormatStat(CoreStatId id, float v)
    {
        // Prozent-Stats (0..1)
        if (id == CoreStatId.CritChance || id == CoreStatId.LifeSteal || id == CoreStatId.Thorns)
            return $"{Mathf.RoundToInt(v * 100f)}%";

        // Multiplikator/Rate-Stats
        if (id == CoreStatId.AttackSpeed || id == CoreStatId.XPGain || id == CoreStatId.MoveSpeed)
            return v.ToString("0.00");

        // Rest
        return v.ToString("0.##");
    }
}


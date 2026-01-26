using System;
using TMPro;
using UnityEngine;

public class StatPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform statsContent;  
    [SerializeField] private StatRowUI statRowPrefab;  

    [Header("References")]
    [SerializeField] private PlayerStatsManager statsManager; 
    [SerializeField] private PlayerProgress progress;         

    [Header("Options")]
    [SerializeField] private bool rebuildOnEnable = true;
    [SerializeField] private bool showProgressRows = true;

    private void OnEnable()
    {
        if (rebuildOnEnable)
            Refresh();
    }
    
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
       
        if (id == CoreStatId.CritChance || id == CoreStatId.LifeSteal || id == CoreStatId.Thorns)
            return $"{Mathf.RoundToInt(v * 100f)}%";

      
        if (id == CoreStatId.AttackSpeed || id == CoreStatId.XPGain || id == CoreStatId.MoveSpeed)
            return v.ToString("0.00");

       
        return v.ToString("0.##");
    }
}


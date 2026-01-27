using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Transform statsContent;          
    [SerializeField] private StatRowUI statRowPrefab;         

    [Header("References")]
    [SerializeField] private PlayerStatsManager statsManager; 
    [SerializeField] private PlayerProgress progress;         

    [Header("Locks")]
    [SerializeField] private LevelUpSkillChoiceController skillMenu;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = true;
    [SerializeField] private MonoBehaviour[] disableWhenOpen; 

    private bool isOpen;

    private void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        isOpen = false;
    }

   
    public void TogglePause(InputAction.CallbackContext ctx)
    {
        Debug.Log($"ESC action fired: phase={ctx.phase} performed={ctx.performed}");
        if (!ctx.performed) return;

        if (skillMenu != null && skillMenu.IsOpen)
            return;

        Toggle();
    }


    public void Toggle()
    {
        if (!isOpen) Open();
        else Close();
    }

    public void Open()
    {
        if (isOpen) return;
        
        if (skillMenu != null && skillMenu.IsOpen)
            return;

        isOpen = true;

        if (pausePanel) pausePanel.SetActive(true);

        if (pauseGameOnOpen)
            Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (disableWhenOpen != null)
            foreach (var comp in disableWhenOpen)
                if (comp) comp.enabled = false;

        RebuildStatsUI();
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (pausePanel) pausePanel.SetActive(false);

        if (pauseGameOnOpen)
            Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (disableWhenOpen != null)
            foreach (var comp in disableWhenOpen)
                if (comp) comp.enabled = true;
    }

    private void RebuildStatsUI()
    {
        if (!statsContent || !statRowPrefab || statsManager == null) return;
        
        for (int i = statsContent.childCount - 1; i >= 0; i--)
            Destroy(statsContent.GetChild(i).gameObject);
        
        if (progress != null)
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


using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Transform statsContent;          // Content der ScrollView
    [SerializeField] private StatRowUI statRowPrefab;         // Prefab für eine Zeile

    [Header("References")]
    [SerializeField] private PlayerStatsManager statsManager; // liest Werte über GetValue
    [SerializeField] private PlayerProgress progress;         // optional (Level/XP etc.)

    [Header("Locks")]
    [Tooltip("Wenn dieses Menü offen ist (z.B. Skill-Menu), darf Pause nicht geöffnet werden.")]
    [SerializeField] private LevelUpSkillChoiceController skillMenu;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = true;
    [SerializeField] private MonoBehaviour[] disableWhenOpen; // optional

    private bool isOpen;

    private void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        isOpen = false;
    }

    // Input System Callback (ESC)
    public void TogglePause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // BLOCK: Pause darf nicht öffnen, wenn Skill-Menü offen ist
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

        // Zusätzliche Sicherheit: auch direkte Open()-Calls blocken
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

        // alte Zeilen löschen
        for (int i = statsContent.childCount - 1; i >= 0; i--)
            Destroy(statsContent.GetChild(i).gameObject);

        // optional: Progress oben anzeigen (Level/XP)
        if (progress != null)
        {
            AddRow("Level", progress.Level.ToString());
            AddRow("Skill Points", progress.SkillPoints.ToString());
        }

        // alle CoreStats anzeigen
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
        // Prozent-Stats (0..1)
        if (id == CoreStatId.CritChance || id == CoreStatId.LifeSteal || id == CoreStatId.Thorns)
            return $"{Mathf.RoundToInt(v * 100f)}%";

        // Multiplikator-Stats (optional)
        if (id == CoreStatId.AttackSpeed || id == CoreStatId.XPGain || id == CoreStatId.MoveSpeed)
            return v.ToString("0.00");

        // Rest
        return v.ToString("0.##");
    }
}


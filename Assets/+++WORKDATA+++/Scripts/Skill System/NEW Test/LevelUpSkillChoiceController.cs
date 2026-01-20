using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelUpSkillChoiceController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private SkillTree tree;

    [Header("Level-based Pools (optional)")]
    [SerializeField] private LevelUpSkillPoolConfig poolConfig;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private SkillChoiceButton[] choiceButtons; // 3

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = true;
    [SerializeField] private MonoBehaviour[] disableWhenOpen;

    [Header("Win Condition")]
    [Tooltip("How many level-up choice panels must the player complete to win.")]
    [Min(1)]
    [SerializeField] private int panelsToWin = 10;

    [Tooltip("Optional: shown when player wins.")]
    [SerializeField] private GameObject winPanel;

    [Tooltip("Optional: triggered when player wins (load scene, etc.).")]
    public UnityEvent onWin;

    private bool isOpen;
    public bool IsOpen => isOpen; // <- WICHTIG: Status nach außen (für Pause-Lock)

    private int pendingLevelUps;
    private readonly Queue<int> pendingLevels = new Queue<int>();

    private int panelsShown;
    private bool hasWon;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        isOpen = false;
        pendingLevelUps = 0;
        panelsShown = 0;
        hasWon = false;

        if (progress != null)
            progress.OnLevelUp += HandleLevelUp;
    }

    void OnDestroy()
    {
        if (progress != null)
            progress.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int newLevel)
    {
        if (hasWon) return;

        pendingLevelUps++;
        pendingLevels.Enqueue(newLevel);

        if (!isOpen)
            OpenAndShowChoicesForNextPendingLevel();
    }

    private void OpenAndShowChoicesForNextPendingLevel()
    {
        if (hasWon) return;
        if (tree == null || progress == null || panelRoot == null) return;
        if (pendingLevels.Count == 0) return;

        isOpen = true;
        panelRoot.SetActive(true);
        ApplyOpenState(true);

        // count a "panel completed" as each time we present a choice set
        panelsShown++;

        // Win check happens when panel is shown (you can change to "after pick" if you prefer)
        if (panelsShown >= panelsToWin)
        {
            TriggerWin();
            return;
        }

        int level = pendingLevels.Peek();

        // Determine pool
        List<SkillDefinition> choices;
        if (poolConfig != null && poolConfig.TryGetPoolForLevel(level, out var poolSkills, out var allowFallback))
        {
            choices = tree.GetRandomUnlockableSkillsWeightedFrom(poolSkills, 3, allowFallback);
        }
        else
        {
            // default: global pool
            choices = tree.GetRandomUnlockableSkillsWeightedFrom(tree.allSkills, 3, allowFallbackToGlobal: false);
        }

        // If pool yields fewer than 3, buttons will be disabled for missing items
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var btn = choiceButtons[i];
            if (btn == null) continue;

            SkillDefinition skill = (i < choices.Count) ? choices[i] : null;
            btn.Setup(skill, OnPickSkill, interactable: (skill != null));
        }
    }

    private void OnPickSkill(SkillDefinition skill)
    {
        if (hasWon) return;
        if (skill == null || tree == null) return;

        bool ok = tree.TryUnlock(skill);
        if (!ok)
        {
            // reroll same level
            OpenAndShowChoicesForNextPendingLevel();
            return;
        }

        // consume one pending level
        if (pendingLevels.Count > 0) pendingLevels.Dequeue();
        pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);

        if (pendingLevelUps > 0)
        {
            OpenAndShowChoicesForNextPendingLevel();
        }
        else
        {
            Close();
        }
    }

    private void TriggerWin()
    {
        hasWon = true;

        // close choice UI
        if (panelRoot != null)
            panelRoot.SetActive(false);

        // keep game paused by default when winning
        if (pauseGameOnOpen)
            Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = false;
        }

        if (winPanel != null)
            winPanel.SetActive(true);

        onWin?.Invoke();
    }

    private void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        ApplyOpenState(false);
    }

    private void ApplyOpenState(bool open)
    {
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (pauseGameOnOpen)
            Time.timeScale = open ? 0f : 1f;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = !open;
        }
    }
}




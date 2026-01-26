using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelUpSkillChoiceController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private SkillTree tree;

    [Header("Level-based Pools (optional)")]
    [SerializeField] private LevelUpSkillPoolConfig poolConfig;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private SkillChoiceButton[] choiceButtons; 

    [Header("Refresh")]
    [Tooltip("Optional: Assign a UI Button to allow one reroll per shown panel.")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private bool allowRefreshOncePerPanel = true;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = true;
    [SerializeField] private MonoBehaviour[] disableWhenOpen;

    [Header("Disable Root While Open")]
    [Tooltip("Dieses Root-GameObject wird deaktiviert, solange das Skill-Panel offen ist, und beim Schließen wiederhergestellt.")]
    [SerializeField] private GameObject rootToDisableWhileOpen;

    [Header("Win Condition")]
    [Tooltip("How many level-up choice panels must the player complete to win.")]
    [Min(1)]
    [SerializeField] private int panelsToWin = 10;

    [Tooltip("Optional: shown when player wins.")]
    [SerializeField] private GameObject winPanel;

    [Tooltip("Optional: triggered when player wins (load scene, etc.).")]
    public UnityEvent onWin;

    private bool isOpen;
    public bool IsOpen => isOpen;

    private int pendingLevelUps;
    private readonly Queue<int> pendingLevels = new Queue<int>();

    private int panelsShown;
    private bool hasWon;

   
    private bool refreshUsedThisPanel;

    
    private bool rootPrevActive;
    private bool rootStateCached;

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
        refreshUsedThisPanel = false;

       
        rootStateCached = false;

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshPressed);
            refreshButton.gameObject.SetActive(true);
        }
        UpdateRefreshButton();

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
            OpenAndShowChoicesForNextPendingLevel(countAsPanel: true);
    }

    private void OpenAndShowChoicesForNextPendingLevel(bool countAsPanel)
    {
        if (hasWon) return;
        if (tree == null || progress == null || panelRoot == null) return;
        if (pendingLevels.Count == 0) return;

        if (!isOpen)
        {
            isOpen = true;
            panelRoot.SetActive(true);

            
            SetRootDisabledWhileOpen(true);

            ApplyOpenState(true);
        }
        else
        {
            panelRoot.SetActive(true);

            
            SetRootDisabledWhileOpen(true);
        }

        if (countAsPanel)
        {
            panelsShown++;

            
            refreshUsedThisPanel = false;

            if (panelsShown >= panelsToWin)
            {
                TriggerWin();
                return;
            }
        }

        int level = pendingLevels.Peek();

        List<SkillDefinition> choices;
        if (poolConfig != null && poolConfig.TryGetPoolForLevel(level, out var poolSkills, out var allowFallback))
        {
            choices = tree.GetRandomUnlockableSkillsWeightedFrom(poolSkills, 3, allowFallback);
        }
        else
        {
            choices = tree.GetRandomUnlockableSkillsWeightedFrom(tree.allSkills, 3, allowFallbackToGlobal: false);
        }

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var btn = choiceButtons[i];
            if (btn == null) continue;

            SkillDefinition skill = (i < choices.Count) ? choices[i] : null;
            btn.Setup(skill, OnPickSkill, interactable: (skill != null));
        }

        UpdateRefreshButton();
    }

    private void OnPickSkill(SkillDefinition skill)
    {
        if (hasWon) return;
        if (skill == null || tree == null) return;

        bool ok = tree.TryUnlock(skill);
        if (!ok)
        {
            OpenAndShowChoicesForNextPendingLevel(countAsPanel: false);
            return;
        }

        if (pendingLevels.Count > 0) pendingLevels.Dequeue();
        pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);

        if (pendingLevelUps > 0)
        {
            OpenAndShowChoicesForNextPendingLevel(countAsPanel: true);
        }
        else
        {
            Close();
        }
    }

    public void OnRefreshPressed()
    {
        if (!allowRefreshOncePerPanel) return;
        if (hasWon) return;
        if (!isOpen) return;
        if (refreshUsedThisPanel) return;
        if (pendingLevels.Count == 0) return;

        refreshUsedThisPanel = true;

        OpenAndShowChoicesForNextPendingLevel(countAsPanel: false);
        UpdateRefreshButton();
    }

    private void UpdateRefreshButton()
    {
        if (refreshButton == null) return;

        refreshButton.interactable =
            allowRefreshOncePerPanel &&
            isOpen &&
            !hasWon &&
            !refreshUsedThisPanel &&
            pendingLevels.Count > 0;
    }

    private void TriggerWin()
    {
        hasWon = true;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        
        if (pauseGameOnOpen)
            Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = false;
        }

        
        SetRootDisabledWhileOpen(true);

        if (winPanel != null)
            winPanel.SetActive(true);

        UpdateRefreshButton();
        onWin?.Invoke();
    }

    private void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        ApplyOpenState(false);

      
        SetRootDisabledWhileOpen(false);

        UpdateRefreshButton();
    }

    private void ApplyOpenState(bool open)
    {
        Debug.Log($"[LevelUp] ApplyOpenState(open={open})");

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

    
    private void SetRootDisabledWhileOpen(bool open)
    {
        if (rootToDisableWhileOpen == null) return;

        if (open)
        {
            
            if (!rootStateCached)
            {
                rootPrevActive = rootToDisableWhileOpen.activeSelf;
                rootStateCached = true;
            }

           
            if (rootToDisableWhileOpen.activeSelf)
                rootToDisableWhileOpen.SetActive(false);
        }
        else
        {
            
            if (rootStateCached)
            {
                rootToDisableWhileOpen.SetActive(rootPrevActive);
                rootStateCached = false;
            }
        }
    }
}



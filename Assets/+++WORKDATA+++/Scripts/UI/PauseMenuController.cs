using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private RectTransform statsContent;
    [SerializeField] private StatRowUI statRowPrefab;

    [Header("References")]
    [SerializeField] private PlayerStatsManager statsManager;
    [SerializeField] private PlayerProgress progress;

    [Header("Input (optional)")]
    [SerializeField] private InputActionReference escAction;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = true;

    [Header("Stats UI")]
    [Tooltip("Wenn true: 0-Werte werden ausgeblendet (außer Always Show Stats).")]
    [SerializeField] private bool hideZeroStats = true;

    [Tooltip("Diese Stats werden immer angezeigt, auch wenn sie 0 sind.")]
    [SerializeField] private CoreStatId[] alwaysShowStats = new[]
    {
        CoreStatId.TurretCount,
        CoreStatId.HPRegen,
        CoreStatId.LifeSteal
    };

    private bool isOpen;

    private void Awake()
    {
        if (escAction != null && escAction.action != null)
            escAction.action.performed += TogglePause;
    }

    private void OnEnable()
    {
        if (escAction != null && escAction.action != null)
            escAction.action.Enable();

        // Falls das Panel per SetActive(true) aktiviert wird:
        if (pausePanel != null && pausePanel.activeInHierarchy)
            RebuildStatsUI();
    }

    private void OnDisable()
    {
        if (escAction != null && escAction.action != null)
            escAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (escAction != null && escAction.action != null)
            escAction.action.performed -= TogglePause;
    }

    private void TogglePause(InputAction.CallbackContext ctx)
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        isOpen = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseGameOnOpen)
            Time.timeScale = 0f;

        RebuildStatsUI();
    }

    public void Close()
    {
        isOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RebuildStatsUI()
    {
        if (statsContent == null || statRowPrefab == null || statsManager == null)
        {
            Debug.LogWarning(
                $"[PauseMenu] Missing references:\n" +
                $"- statsContent null? {(statsContent == null)}\n" +
                $"- statRowPrefab null? {(statRowPrefab == null)}\n" +
                $"- statsManager null? {(statsManager == null)}",
                this
            );
            return;
        }

        // Clear
        for (int i = statsContent.childCount - 1; i >= 0; i--)
            Destroy(statsContent.GetChild(i).gameObject);

        // Build
        foreach (CoreStatId statId in Enum.GetValues(typeof(CoreStatId)))
        {
            float value = statsManager.GetValue(statId);

            bool alwaysShow = ArrayContains(alwaysShowStats, statId);

            if (hideZeroStats && !alwaysShow && Mathf.Approximately(value, 0f))
                continue;

            string name = StatTextUtil.GetDisplayName(statId);
            string formatted = StatTextUtil.FormatValue(statId, value);

            var row = Instantiate(statRowPrefab, statsContent);
            row.name = $"StatRow_{statId}";
            row.Set(name, formatted);
        }

        Canvas.ForceUpdateCanvases();
    }

    private static bool ArrayContains(CoreStatId[] arr, CoreStatId value)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].Equals(value)) return true;
        return false;
    }
}



using UnityEngine;
using UnityEngine.InputSystem;

public class SkillMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject skillMenuPanel;
    [SerializeField] private bool pauseGameOnOpen = true;

    [Header("Player Progress")]
    [SerializeField] private PlayerProgress progress;   // NEU: Referenz setzen!

    [Header("Player Control (optional)")]
    [SerializeField] private MonoBehaviour[] disableWhenOpen;

    [Header("Behavior")]
    [SerializeField] private bool autoCloseWhenZero = true; // optional

    private bool isOpen;
    private bool lockOpen; // solange SkillPoints > 0

    void Start()
    {
        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(false);

        isOpen = false;
        lockOpen = false;

        if (progress != null)
        {
            progress.OnSkillPointsChanged += HandleSkillPointsChanged;

            // Initialer Zustand
            HandleSkillPointsChanged(progress.SkillPoints);
        }
    }

    void OnDestroy()
    {
        if (progress != null)
            progress.OnSkillPointsChanged -= HandleSkillPointsChanged;
    }

    private void HandleSkillPointsChanged(int points)
    {
        lockOpen = points > 0;

        if (lockOpen)
        {
            // automatisch öffnen
            Open();
        }
        else
        {
            // Punkte sind 0 -> Menü darf schließen
            if (autoCloseWhenZero)
                Close();
        }
    }

    // Input System Callback
    public void ToggleSkillMenu(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Toggle();
    }

    public void Toggle()
    {
        // Wenn noch SkillPoints übrig sind: NICHT schließen lassen
        if (isOpen && lockOpen)
            return;

        if (!isOpen) Open();
        else Close();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(true);

        ApplyOpenState();
    }

    public void Close()
    {
        if (!isOpen) return;

        // Sicherheit: nicht schließen solange lockOpen aktiv
        if (lockOpen) return;

        isOpen = false;

        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(false);

        ApplyOpenState();
    }

    private void ApplyOpenState()
    {
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        if (pauseGameOnOpen)
            Time.timeScale = isOpen ? 0f : 1f;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = !isOpen;
        }
    }
}


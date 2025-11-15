using UnityEngine;
using UnityEngine.InputSystem;

public class SkillMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject skillMenuPanel;   // dein Panel
    [SerializeField] private bool pauseGameOnOpen = true;

    [Header("Player Control (optional)")]
    [SerializeField] private MonoBehaviour[] disableWhenOpen; // z.B. PlayerMovement, PlayerAttack

    private bool isOpen;

    void Start()
    {
        // Sicherstellen, dass es beim Start aus ist
        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(false);
        isOpen = false;
    }

    // Input System Callback
    public void ToggleSkillMenu(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(isOpen);

        // Cursor
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        // Option: Game pausieren
        if (pauseGameOnOpen)
            Time.timeScale = isOpen ? 0f : 1f;

        // Option: Player-Steuerung deaktivieren
        foreach (var comp in disableWhenOpen)
        {
            if (comp != null)
                comp.enabled = !isOpen;
        }
    }
}


using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject skillMenuPanel;   // dein Panel
    [SerializeField] private bool pauseGameOnOpen = true;

    [Header("References")]
    [SerializeField] private PlayerProgress player;
    [SerializeField] private SkillTree skillTree;

    [Header("Optional: Player Control")]
    [SerializeField] private MonoBehaviour[] disableWhenOpen; // z.B. Movement/Attack Scripts

    private bool isOpen;

    private void Awake()
    {
        // Panel beim Start geschlossen
        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(false);

        isOpen = false;
    }

    private void OnEnable()
    {
        if (player != null)
            player.OnLevelUp += HandleLevelUp;

        if (skillTree != null)
            skillTree.OnSkillUnlocked += HandleSkillUnlocked;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnLevelUp -= HandleLevelUp;

        if (skillTree != null)
            skillTree.OnSkillUnlocked -= HandleSkillUnlocked;
    }

    // 🔔 Player leveled up → Skilltree öffnen
    private void HandleLevelUp(int newLevel)
    {
        SetOpen(true);
    }

    // 🔔 Skill freigeschaltet → Skilltree schließen
    private void HandleSkillUnlocked(SkillDefinition skill)
    {
        SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (skillMenuPanel != null)
            skillMenuPanel.SetActive(isOpen);

        // Cursor
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        // Optional: Spiel pausieren
        if (pauseGameOnOpen)
            Time.timeScale = isOpen ? 0f : 1f;

        // Optional: Spieler-Steuerung deaktivieren
        foreach (var comp in disableWhenOpen)
        {
            if (comp != null)
                comp.enabled = !isOpen;
        }
    }
}

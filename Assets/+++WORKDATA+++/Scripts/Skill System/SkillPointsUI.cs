using TMPro;
using UnityEngine;

public class SkillPointsUI : MonoBehaviour
{
    public PlayerProgress progress;          // Reference to your PlayerProgress
    public TextMeshProUGUI textUI;           // The text object that displays the skill points

    void Start()
    {
        UpdateUI(); // Initial display
    }

    void Update()
    {
        UpdateUI(); // Always update when value changes
    }

    void UpdateUI()
    {
        if (progress == null || textUI == null) return;

        textUI.text = $"Skill Points: {progress.SkillPoints}";
    }
}


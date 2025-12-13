using TMPro;
using UnityEngine;

public class SkillPointsUI : MonoBehaviour
{
    public PlayerProgress progress;
    public TextMeshProUGUI textUI;

    void Start()
    {
        if (progress != null)
            progress.OnSkillPointsChanged += OnChanged;

        OnChanged(progress != null ? progress.SkillPoints : 0);
    }

    void OnDestroy()
    {
        if (progress != null)
            progress.OnSkillPointsChanged -= OnChanged;
    }

    void OnChanged(int points)
    {
        if (textUI == null) return;
        textUI.text = $"Skill Points: {points}";
    }
}


// SkillChoiceButton.cs (FINAL: provides Setup(...) used by LevelUpSkillChoiceController,
// does NOT touch colors; delegates visuals to SkillButtonUI)
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillChoiceButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text subtitle;
    [SerializeField] private Image icon;

    [Header("Optional: Rarity UI (colors)")]
    [SerializeField] private SkillButtonUI rarityUI;

    private Button btn;
    private SkillDefinition boundSkill;

    private void Awake()
    {
        btn = GetComponent<Button>();

        // Auto-wire rarityUI if not set
        if (!rarityUI) rarityUI = GetComponent<SkillButtonUI>();
    }

    /// <summary>
    /// This matches your LevelUpSkillChoiceController call:
    /// btn.Setup(skill, OnPickSkill, interactable: (skill != null));
    /// </summary>
    public void Setup(SkillDefinition skill, Action<SkillDefinition> onPicked, bool interactable)
    {
        boundSkill = skill;

        // Interactable state
        if (!btn) btn = GetComponent<Button>();
        btn.interactable = interactable && (skill != null);

        // Text/Icon
        if (title) title.text = skill ? skill.displayName : "";
        if (subtitle) subtitle.text = skill ? skill.description : "";
        if (icon)
        {
            icon.enabled = (skill != null && skill.icon != null);
            icon.sprite = (skill != null) ? skill.icon : null;
        }

        // Rarity coloring handled here (SkillButtonUI). SkillChoiceButton itself does not modify colors.
        if (rarityUI != null && skill != null)
            rarityUI.Bind(skill);

        // Click
        btn.onClick.RemoveAllListeners();
        if (skill != null && onPicked != null)
        {
            btn.onClick.AddListener(() => onPicked(boundSkill));
        }
    }

    public SkillDefinition GetBoundSkill() => boundSkill;
}

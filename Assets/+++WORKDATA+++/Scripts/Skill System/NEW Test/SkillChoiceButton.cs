using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private Image icon;

    [Header("Tooltip")]
    [SerializeField] private SkillTooltipUI tooltip;

    [Header("Per-Button Info Text (Inspector)")]
    [TextArea]
    [SerializeField] private string buttonInfoText;

    [Tooltip("Wenn true: hängt automatisch die Skill.description an den Button-Text an.")]
    [SerializeField] private bool appendSkillDescription = true;

    private Button btn;
    private SkillDefinition boundSkill;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    public void Setup(SkillDefinition skill, Action<SkillDefinition> onPicked, bool interactable)
    {
        boundSkill = skill;

        btn.interactable = interactable && skill != null;

        if (title) title.text = skill ? skill.displayName : "";
        if (icon)
        {
            icon.enabled = skill && skill.icon;
            icon.sprite = skill ? skill.icon : null;
        }

        btn.onClick.RemoveAllListeners();
        if (skill != null && onPicked != null)
            btn.onClick.AddListener(() => onPicked(boundSkill));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || boundSkill == null) return;

        // Button-eigener Text (pro Prefab/Instanz einstellbar)
        string info = buttonInfoText ?? "";

        if (appendSkillDescription)
        {
            string desc = boundSkill.description ?? "";
            if (!string.IsNullOrWhiteSpace(desc))
            {
                if (!string.IsNullOrWhiteSpace(info))
                    info += "\n\n";
                info += desc;
            }
        }

        tooltip.ShowSkill(boundSkill, info);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetDefault();
    }
}




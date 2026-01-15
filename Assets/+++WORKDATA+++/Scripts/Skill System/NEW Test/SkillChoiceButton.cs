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
    [SerializeField] private TMP_Text subtitle;
    [SerializeField] private Image icon;

    [Header("Tooltip (optional)")]
    [SerializeField] private SkillTooltipUI tooltip;

    private Button btn;
    private SkillDefinition current;
    private Action<SkillDefinition> onPick;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (current != null)
                onPick?.Invoke(current);
        });
    }

    public void Setup(SkillDefinition skill, Action<SkillDefinition> pickCallback, bool interactable)
    {
        current = skill;
        onPick = pickCallback;

        if (title) title.text = skill != null ? skill.displayName : "—";
        if (subtitle) subtitle.text = skill != null ? skill.description : "";

        if (icon)
        {
            icon.sprite = (skill != null) ? skill.icon : null;
            icon.enabled = (skill != null && skill.icon != null);
        }

        if (btn) btn.interactable = interactable && skill != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null) return;
        if (current == null) { tooltip.Hide(); return; }
        tooltip.ShowFor(current);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }
}
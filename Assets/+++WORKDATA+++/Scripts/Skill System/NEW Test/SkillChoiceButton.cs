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
    [SerializeField] private TMP_Text infoText;          // <-- Info-Text bleibt erhalten
    [SerializeField] private Image icon;

    [Header("Rarity Visuals")]
    [SerializeField] private Image background;
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = Color.green;

    [Header("Tooltip")]
    [SerializeField] private SkillTooltipUI tooltip;

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

        // Titel
        if (title)
            title.text = skill ? skill.displayName : "";

        // Info-Text
        if (infoText)
            infoText.text = skill ? skill.description : "";

        // Icon
        if (icon)
        {
            icon.enabled = skill && skill.icon;
            icon.sprite = skill ? skill.icon : null;
        }

        // Rarity-Farbe anwenden
        ApplyRarityColor(skill);

        btn.onClick.RemoveAllListeners();
        if (skill != null && onPicked != null)
            btn.onClick.AddListener(() => onPicked(boundSkill));
    }

    private void ApplyRarityColor(SkillDefinition skill)
    {
        if (background == null) return;

        if (skill == null)
        {
            background.color = commonColor;
            return;
        }

        switch (skill.rarity)
        {
            case SkillRarity.Common:
                background.color = commonColor;
                break;

            case SkillRarity.Uncommon:
                background.color = uncommonColor;
                break;

            default:
                background.color = commonColor;
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || boundSkill == null) return;
        tooltip.ShowSkill(boundSkill, boundSkill.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetDefault();
    }
}







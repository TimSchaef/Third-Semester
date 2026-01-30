using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillButtonUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image background; 
    [SerializeField] private Image icon;       

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = Color.green;

    private Button btn;
    private SkillDefinition skill;

    private void Awake()
    {
        btn = GetComponent<Button>();

        
        btn.transition = Selectable.Transition.None;
    }

    public void Bind(SkillDefinition skillDef)
    {
        skill = skillDef;
        if (!skill) return;

        if (icon) icon.sprite = skill.icon;

        if (!background) return;

        background.color = skill.rarity switch
        {
            SkillRarity.Common => commonColor,
            SkillRarity.Uncommon => uncommonColor,
            _ => commonColor
        };
    }

    public SkillDefinition GetSkill() => skill;
}




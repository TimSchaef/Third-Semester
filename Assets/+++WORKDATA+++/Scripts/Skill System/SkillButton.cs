using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SkillButton : MonoBehaviour
{
    public SkillTree tree;
    public SkillDefinition skill;

    [Header("UI (optional)")]
    public TMP_Text title;
    public TMP_Text subtitle;
    public Image icon;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void Start()
    {
        Refresh();
        if (tree != null)
        {
            tree.OnSkillUnlocked += _ => Refresh();
        }
    }

    void OnEnable() => Refresh();

    void Refresh()
    {
        if (skill == null || tree == null) return;

        if (title) title.text = skill.displayName;
        if (subtitle)
        {
            string reason;
            bool can = tree.CanUnlock(skill, out reason);
            if (tree.IsUnlocked(skill)) reason = "Unlocked";
            subtitle.text = tree.IsUnlocked(skill)
                ? "Unlocked"
                : can ? $"Cost: {skill.costSkillPoints} | Req Lvl: {skill.requiredPlayerLevel}"
                    : reason;
        }
        if (icon) icon.sprite = skill.icon;

        string _;
        btn.interactable = !tree.IsUnlocked(skill) && tree.CanUnlock(skill, out _);
    }

    void OnClick()
    {
        if (tree.TryUnlock(skill))
            Refresh();
    }
}


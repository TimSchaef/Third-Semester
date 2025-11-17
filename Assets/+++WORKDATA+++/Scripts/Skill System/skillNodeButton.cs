using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SkillNodeRequirementMode
{
    All,   // alle vorherigen Buttons müssen unlocked sein
    Any    // mindestens einer der vorherigen Buttons
}

[RequireComponent(typeof(Button))]
public class SkillNodeButton : MonoBehaviour
{
    [Header("Refs")]
    public SkillTree tree;
    public SkillDefinition skill;

    [Header("Node Connections")]
    public SkillNodeRequirementMode requirementMode = SkillNodeRequirementMode.All;
    public List<SkillNodeButton> prerequisiteNodes = new List<SkillNodeButton>();

    [Header("UI")]
    public TMP_Text title;
    public TMP_Text subtitle;
    public Image icon;

    // 👉 Hier: dieser Button weiß, ob ER selbst freigeschaltet ist
    [HideInInspector] 
    public bool unlocked = false;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void Start()
    {
        if (tree != null)
            tree.OnSkillUnlocked += _ => Refresh();

        Refresh();
    }

    void OnEnable() => Refresh();

    // Prüft nur, ob die VORGÄNGER-BUTTONS unlocked sind
    bool LocalPrereqsMet()
    {
        var valid = prerequisiteNodes.Where(p => p != null).ToList();
        if (valid.Count == 0) return true; // keine Vorgänger -> frei

        switch (requirementMode)
        {
            case SkillNodeRequirementMode.All:
                return valid.All(n => n.unlocked);
            case SkillNodeRequirementMode.Any:
                return valid.Any(n => n.unlocked);
            default:
                return true;
        }
    }

    void Refresh()
    {
        if (skill == null || tree == null || btn == null) return;

        if (title) title.text = skill.displayName;
        if (icon) icon.sprite = skill.icon;

        bool prereqsOK = LocalPrereqsMet();

        string reason;
        bool canByLevelAndPoints = tree.CanUnlock(skill, out reason);

        if (subtitle)
        {
            if (unlocked)
            {
                subtitle.text = "Unlocked";
            }
            else if (!prereqsOK)
            {
                subtitle.text = requirementMode == SkillNodeRequirementMode.All
                    ? "Requires all previous skills."
                    : "Requires one of the previous skills.";
            }
            else if (!canByLevelAndPoints)
            {
                subtitle.text = reason;
            }
            else
            {
                subtitle.text = $"Cost: {skill.costSkillPoints} | Req Lvl: {skill.requiredPlayerLevel}";
            }
        }

        // 👉 Button ist nur klickbar, wenn:
        // - dieser Button noch NICHT unlocked ist
        // - alle vorherigen Buttons erfüllt sind
        // - genug Level + SkillPoints da sind
        btn.interactable = !unlocked && prereqsOK && canByLevelAndPoints;
    }

    void OnClick()
    {
        if (tree == null || skill == null) return;

        if (!tree.TryUnlock(skill))
        {
            string reason;
            tree.CanUnlock(skill, out reason);
            Debug.Log($"[SkillNodeButton] Click on {skill.skillId} failed: {reason}");
            return;
        }

        // 👉 AB HIER gilt dieser BUTTON als freigeschaltet
        unlocked = true;
        Refresh();
    }
}





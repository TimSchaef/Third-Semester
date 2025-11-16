using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SkillNodeRequirementMode
{
    All,   // alle vorherigen Skills müssen freigeschaltet sein (UND)
    Any    // mindestens einer der vorherigen Skills (ODER)
}

[RequireComponent(typeof(Button))]
public class SkillNodeButton : MonoBehaviour
{
    [Header("Refs")]
    public SkillTree tree;                 // dein SkillSystem-Objekt
    public SkillDefinition skill;          // Stat-Effekt-Asset (kann mehrfach verwendet werden!)

    [Header("Node Connections")]
    public SkillNodeRequirementMode requirementMode = SkillNodeRequirementMode.All;
    public List<SkillNodeButton> prerequisiteNodes = new List<SkillNodeButton>();
    // → Hier verlinkst du im Inspector die Buttons, die VOR diesem Button kommen.

    [Header("UI")]
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
        if (tree != null)
            tree.OnSkillUnlocked += _ => Refresh();  // bei jedem Unlock neu prüfen

        Refresh();
    }

    void OnEnable() => Refresh();

    void OnDestroy()
    {
        if (tree != null)
            tree.OnSkillUnlocked -= _ => Refresh();
    }

    bool LocalPrereqsMet()
    {
        var valid = prerequisiteNodes.Where(p => p != null && p.skill != null).ToList();
        if (valid.Count == 0) return true; // keine Vorgänger → sofort erlaubt

        // Prüfe, ob die Vorgänger-Skills freigeschaltet sind
        switch (requirementMode)
        {
            case SkillNodeRequirementMode.All:
                return valid.All(n => tree.IsUnlocked(n.skill));
            case SkillNodeRequirementMode.Any:
                return valid.Any(n => tree.IsUnlocked(n.skill));
            default:
                return true;
        }
    }

    void Refresh()
    {
        if (skill == null || tree == null || btn == null) return;

        if (title) title.text = skill.displayName;
        if (icon) icon.sprite = skill.icon;

        bool unlocked = tree.IsUnlocked(skill);
        bool prereqsOK = LocalPrereqsMet();

        string reason;
        bool canByLevelAndPoints = tree.CanUnlock(skill, out reason); // prüft nur Level + SkillPoints + evtl. eigene skill.prerequisites (am besten leer lassen)

        // UI-Text
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

        // Button aktiv/deaktiv
        btn.interactable = !unlocked && prereqsOK && canByLevelAndPoints;
    }

    void OnClick()
    {
        if (tree.TryUnlock(skill))
        {
            Refresh();
        }
    }
}

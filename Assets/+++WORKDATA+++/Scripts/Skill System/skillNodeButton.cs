using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

    // 👉 NUR noch diese Liste
    public List<SkillDefinition> skills = new List<SkillDefinition>();

    [Header("Node Connections")]
    public SkillNodeRequirementMode requirementMode = SkillNodeRequirementMode.All;
    public List<SkillNodeButton> prerequisiteNodes = new List<SkillNodeButton>();

    [Header("UI")]
    public TMP_Text title;
    public TMP_Text subtitle;
    public Image icon;
    
    [Header("Tooltip")]
    public SkillTooltipUI tooltip;

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

    bool LocalPrereqsMet()
    {
        var valid = prerequisiteNodes.Where(p => p != null).ToList();
        if (valid.Count == 0) return true;

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

    bool CanUnlockAll(out string reason)
    {
        reason = "";

        if (tree == null) { reason = "No SkillTree reference."; return false; }
        if (skills == null || skills.Count == 0) { reason = "No skills assigned."; return false; }

        foreach (var s in skills)
        {
            if (s == null) continue;

            if (!tree.CanUnlock(s, out reason))
                return false;
        }

        return true;
    }

    void Refresh()
    {
        if (tree == null || btn == null || skills == null || skills.Count == 0)
            return;

        var primary = skills.FirstOrDefault(s => s != null);
        if (primary == null) return;

        if (title) title.text = primary.displayName;
        if (icon) icon.sprite = primary.icon;

        bool prereqsOK = LocalPrereqsMet();

        string reason;
        bool canByLevelAndPoints = CanUnlockAll(out reason);

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
                if (skills.Count == 1)
                {
                    subtitle.text =
                        $"Cost: {primary.costSkillPoints} | Req Lvl: {primary.requiredPlayerLevel}";
                }
                else
                {
                    int totalCost = skills.Sum(s => s != null ? s.costSkillPoints : 0);
                    int maxReqLvl = skills.Max(s => s != null ? s.requiredPlayerLevel : 0);
                    subtitle.text =
                        $"{skills.Count} Skills | Total Cost: {totalCost} | Req Lvl: {maxReqLvl}";
                }
            }
        }

        btn.interactable = !unlocked && prereqsOK && canByLevelAndPoints;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[Tooltip] Pointer Enter on: " + gameObject.name);
        if (tooltip == null) return;
        if (skills == null || skills.Count == 0) { tooltip.Hide(); return; }

        var arr = skills.Where(s => s != null).ToArray();
        if (arr.Length == 0) { tooltip.Hide(); return; }

        if (arr.Length == 1) tooltip.ShowFor(arr[0]);
        else tooltip.ShowForMultiple(arr);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip == null) return;
        tooltip.Hide();
    }

    void OnClick()
    {
        if (tree == null || skills == null || skills.Count == 0)
            return;

        string reason;
        if (!CanUnlockAll(out reason))
        {
            Debug.Log($"[SkillNodeButton] Click failed: {reason}");
            return;
        }

        foreach (var s in skills)
        {
            if (s == null) continue;

            if (!tree.TryUnlock(s))
            {
                tree.CanUnlock(s, out reason);
                Debug.Log($"[SkillNodeButton] Unlock {s.skillId} failed: {reason}");
                return;
            }
        }
        
        unlocked = true;
        Refresh();
    }
}






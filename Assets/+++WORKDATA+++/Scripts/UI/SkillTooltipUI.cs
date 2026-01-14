using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltipUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;          // TooltipPanel
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text extraText;
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        Hide();
    }

    public void ShowFor(SkillDefinition skill)
    {
        if (skill == null) { Hide(); return; }

        if (root) root.SetActive(true);

        if (titleText) titleText.text = skill.displayName;
        if (descText) descText.text = skill.description;

        if (iconImage)
        {
            iconImage.enabled = skill.icon != null;
            iconImage.sprite = skill.icon;
        }

        if (extraText)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Cost: {skill.costSkillPoints}  |  Req Lvl: {skill.requiredPlayerLevel}");

            if (skill.effects != null && skill.effects.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Effects:");
                foreach (var e in skill.effects)
                    sb.AppendLine($"- {e.stat} {e.op} {e.value}");
            }

            extraText.text = sb.ToString().TrimEnd();
        }
    }

    public void ShowForMultiple(SkillDefinition[] skills)
    {
        if (skills == null || skills.Length == 0) { Hide(); return; }

        // pick a primary for title/icon
        var primary = skills.FirstOrDefault(s => s != null);
        if (primary == null) { Hide(); return; }

        if (root) root.SetActive(true);

        if (titleText) titleText.text = primary.displayName + (skills.Length > 1 ? $" (+{skills.Length - 1})" : "");
        if (descText) descText.text = primary.description;

        if (iconImage)
        {
            iconImage.enabled = primary.icon != null;
            iconImage.sprite = primary.icon;
        }

        if (extraText)
        {
            int totalCost = skills.Where(s => s != null).Sum(s => s.costSkillPoints);
            int maxReq = skills.Where(s => s != null).Max(s => s.requiredPlayerLevel);

            var sb = new StringBuilder();
            sb.AppendLine($"{skills.Length} Skills  |  Total Cost: {totalCost}  |  Req Lvl: {maxReq}");

            // list effects grouped per skill
            sb.AppendLine();
            sb.AppendLine("Effects:");
            foreach (var s in skills.Where(s => s != null))
            {
                sb.AppendLine($"{s.displayName}:");
                if (s.effects != null && s.effects.Count > 0)
                {
                    foreach (var e in s.effects)
                        sb.AppendLine($"- {e.stat} {e.op} {e.value}");
                }
                else
                {
                    sb.AppendLine("- (none)");
                }
                sb.AppendLine();
            }

            extraText.text = sb.ToString().TrimEnd();
        }
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }
}


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

[RequireComponent(typeof(Button))]
public class StatUpgradeButton : MonoBehaviour
{
    public PlayerStatsManager statsManager;
    public PlayerStatDefinition stat;
    public TMP_Text title;
    public TMP_Text subtitle;

    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnEnable() => Refresh();

    void Refresh()
    {
        int lv = statsManager.GetLevel(stat.statId);
        title.text = $"{stat.displayName} (Lv {lv}/{stat.maxLevel})";

        float cur = statsManager.GetValue(stat.statId);
        float next = stat.GetValueAtLevel(Mathf.Min(stat.maxLevel, lv + 1));
        string reason;
        bool can = statsManager.CanUpgrade(stat, out reason);

        subtitle.text = statsManager.CanUpgrade(stat, out _)
            ? $"Aktuell: {cur:0.##}  →  Nächstes: {next:0.##}  | Kosten: {stat.costPerUpgrade}"
            : (lv >= stat.maxLevel ? "Max Level" : reason);

        btn.interactable = can;
    }

    void OnClick()
    {
        if (statsManager.TryUpgrade(stat))
            Refresh();
    }
}


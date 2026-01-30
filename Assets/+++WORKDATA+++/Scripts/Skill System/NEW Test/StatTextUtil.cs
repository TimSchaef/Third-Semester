using System.Collections.Generic;
using UnityEngine;


public class StatTextUtil : MonoBehaviour
{
    private static readonly HashSet<CoreStatId> PercentStats = new()
    {
        CoreStatId.CritChance,
        CoreStatId.CritDamage,
        CoreStatId.LifeSteal,
        CoreStatId.AttackSpeed,
        CoreStatId.XPGain
    };

    public static string GetDisplayName(CoreStatId id)
    {
        return id switch
        {
            CoreStatId.MaxHP => "Max HP",
            CoreStatId.HPRegen => "HP Regen",
            CoreStatId.MoveSpeed => "Move Speed",
            CoreStatId.AttackSpeed => "Attack Speed",
            CoreStatId.AoeDamage => "AOE Damage",
            CoreStatId.AoeRadius => "AOE Radius",
            CoreStatId.AoeTickRate => "AOE Tick Rate",
            CoreStatId.CritChance => "Crit Chance",
            CoreStatId.CritDamage => "Crit Damage",
            CoreStatId.XPGain => "XP Gain",
            CoreStatId.TurretCount => "Turret Count",
            CoreStatId.TurretDamage => "Turret Damage",
            _ => id.ToString()
        };
    }

    public static string FormatValue(CoreStatId id, float value)
    {
        if (id == CoreStatId.TurretCount)
            return Mathf.RoundToInt(value).ToString();

        if (PercentStats.Contains(id))
            return (value * 100f).ToString("0.#") + "%";

        float abs = Mathf.Abs(value);
        if (abs >= 100f) return value.ToString("0");
        if (abs >= 10f) return value.ToString("0.#");
        return value.ToString("0.##");
    }
}
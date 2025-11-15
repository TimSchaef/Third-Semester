using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skill", fileName = "NewSkill")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    public string skillId;          // unique string (e.g., "firebolt")
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Requirements")]
    [Min(1)] public int requiredPlayerLevel = 1;
    [Min(1)] public int costSkillPoints = 1;
    public List<SkillDefinition> prerequisites = new List<SkillDefinition>();
}

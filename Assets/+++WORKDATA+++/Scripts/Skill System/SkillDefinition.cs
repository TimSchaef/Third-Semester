using System.Collections.Generic;
using UnityEngine;

public enum SkillPrerequisiteMode
{
    All,    // alle müssen freigeschaltet sein (UND)
    Any     // mindestens einer muss freigeschaltet sein (ODER)
}

[CreateAssetMenu(menuName = "RPG/Skill", fileName = "NewSkill")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    public string skillId;          // unique string (z.B. "attack_speed_1")
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Requirements")]
    [Min(1)] public int requiredPlayerLevel = 1;
    [Min(1)] public int costSkillPoints = 1;

    // NEU: wie werden die prerequisites ausgewertet?
    public SkillPrerequisiteMode prerequisiteMode = SkillPrerequisiteMode.All;

    // Skills, die vorher freigeschaltet sein müssen
    public List<SkillDefinition> prerequisites = new List<SkillDefinition>();

    [Header("Effects")]
    public List<SkillEffect> effects = new List<SkillEffect>();
}

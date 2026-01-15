using System.Collections.Generic;
using UnityEngine;

public enum SkillPrerequisiteMode
{
    All,
    Any
}

[CreateAssetMenu(menuName = "RPG/Skill", fileName = "NewSkill")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Requirements")]
    [Min(1)] public int requiredPlayerLevel = 1;
    [Min(1)] public int costSkillPoints = 1;

    [Header("Stacking")]
    [Min(0)] public int maxPicks = 0; // 0 = infinite

    [Header("Drop Chance")]
    [Tooltip("Relative weight for appearing in the level-up choices. 0 = never. 1 = normal. 5 = very common.")]
    [Min(0f)] public float dropWeight = 1f;

    [Header("Prerequisites")]
    public SkillPrerequisiteMode prerequisiteMode = SkillPrerequisiteMode.All;
    public List<SkillDefinition> prerequisites = new List<SkillDefinition>();

    [Header("Effects")]
    public List<SkillEffect> effects = new List<SkillEffect>();
}



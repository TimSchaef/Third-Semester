using System.Collections.Generic;
using UnityEngine;

public enum SkillPrerequisiteMode
{
    All,
    Any
}

public enum SkillRarity
{
    Common,
    Uncommon
}

[CreateAssetMenu(menuName = "RPG/Skill", fileName = "NewSkill")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rarity")]
    public SkillRarity rarity = SkillRarity.Common;

    [Header("Requirements")]
    [Min(1)] public int requiredPlayerLevel = 1;
    [Min(1)] public int costSkillPoints = 1;

    [Header("Stacking")]
    [Tooltip("0 = infinite picks")]
    [Min(0)] public int maxPicks = 0;

    [Header("Drop Chance")]
    [Tooltip("Relative weight for appearing in the level-up choices. 0 = never.")]
    [Min(0f)] public float dropWeight = 1f;

    [Header("Prerequisites")]
    public SkillPrerequisiteMode prerequisiteMode = SkillPrerequisiteMode.All;
    public List<SkillDefinition> prerequisites = new();

    [Header("Effects")]
    public List<SkillEffect> effects = new();
}




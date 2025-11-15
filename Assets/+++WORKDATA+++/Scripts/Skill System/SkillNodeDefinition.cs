using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skill Tree/Skill Node", fileName = "SkillNode")]
public class SkillNodeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string nodeId;           // z.B. "swift_legs"
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Unlocking")]
    [Min(1)] public int maxRank = 1;
    [Min(1)] public int costPerRank = 1;
    [Min(1)] public int requiredPlayerLevel = 1;
    public List<SkillNodeDefinition> prerequisites = new(); // alle müssen mind. Rang 1 haben

    [System.Serializable]
    public class RankEffects { public List<SkillEffect> effects = new(); }

    [Header("Effects per Rank")]
    public List<RankEffects> ranks = new(); // Größe = maxRank

    public IEnumerable<SkillEffect> GetEffectsForRank(int rank)
    {
        rank = Mathf.Clamp(rank, 1, maxRank);
        if (ranks == null || ranks.Count < rank) yield break;
        foreach (var e in ranks[rank - 1].effects) yield return e;
    }
}

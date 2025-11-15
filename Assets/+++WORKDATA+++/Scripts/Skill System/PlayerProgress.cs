using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int unspentSkillPoints = 0;

    public int Level => currentLevel;
    public int XP => currentXP;
    public int SkillPoints => unspentSkillPoints;

    private int XPRequiredForNextLevel() => currentLevel * 100;
    
    public int GetXPRequiredForNextLevel()
    {
        return XPRequiredForNextLevel();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        currentXP += amount;
        while (currentXP >= XPRequiredForNextLevel())
        {
            currentXP -= XPRequiredForNextLevel();
            currentLevel++;
            unspentSkillPoints++;
        }
    }

    public void AddXPMultiplied(int baseAmount, float multiplier)
    {
        AddXP(Mathf.RoundToInt(baseAmount * Mathf.Max(0f, multiplier)));
    }

    public bool SpendSkillPoints(int amount)
    {
        if (unspentSkillPoints < amount) return false;
        unspentSkillPoints -= amount;
        return true;
    }
}



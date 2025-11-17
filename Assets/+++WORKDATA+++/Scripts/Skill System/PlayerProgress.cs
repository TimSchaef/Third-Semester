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
    public int GetXPRequiredForNextLevel() => XPRequiredForNextLevel();

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= XPRequiredForNextLevel())
        {
            currentXP -= XPRequiredForNextLevel();

            // LEVEL UP
            currentLevel++;

            // +1 Skillpunkt pro Level-Up
            unspentSkillPoints++;
        }
    }

    // 🔥 HIER: Methode wieder einfügen, die XPGiver & Hitpoints benutzen
    public void AddXPMultiplied(int baseAmount, float multiplier)
    {
        // multiplier z.B. aus XPGain-Stat (1.0 = normal, 1.2 = +20% XP)
        int finalAmount = Mathf.RoundToInt(baseAmount * Mathf.Max(0f, multiplier));
        AddXP(finalAmount);
    }

    public bool SpendSkillPoints(int amount)
    {
        if (unspentSkillPoints < amount) return false;
        unspentSkillPoints -= amount;
        return true;
    }

    // --- SPEICHERN / LADEN ---

    const string KEY_LEVEL = "player_level";
    const string KEY_XP = "player_xp";
    const string KEY_SP = "player_skillpoints";

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(KEY_LEVEL, currentLevel);
        PlayerPrefs.SetInt(KEY_XP, currentXP);
        PlayerPrefs.SetInt(KEY_SP, unspentSkillPoints);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt(KEY_LEVEL, 1);
        currentXP = PlayerPrefs.GetInt(KEY_XP, 0);
        unspentSkillPoints = PlayerPrefs.GetInt(KEY_SP, 0);
    }
}






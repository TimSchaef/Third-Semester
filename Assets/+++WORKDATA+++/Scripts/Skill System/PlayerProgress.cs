using System;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    [Header("Startwerte für einen Run")]
    [SerializeField] private int startLevel = 1;
    [SerializeField] private int startXP = 0;
    [SerializeField] private int startSkillPoints = 0;

    [Header("XP Scaling")]
    [Tooltip("Optional: Wenn gesetzt, wird der XPGain-Stat als Multiplikator eingerechnet.")]
    [SerializeField] private PlayerStatsManager statsManager;

    [Tooltip("Multiplikator pro Level-Up (z.B. 0.10 = +10% pro Level).")]
    [SerializeField, Range(0f, 1f)] private float xpGainBonusPerLevel = 0.10f;

    [Tooltip("Wenn true: XPGain-Stat wird als Multiplikator genutzt (1.0 = normal).")]
    [SerializeField] private bool useXPGainStat = true;

    private int currentLevel;
    private int currentXP;
    private int unspentSkillPoints;

    public int Level => currentLevel;
    public int XP => currentXP;
    public int SkillPoints => unspentSkillPoints;

    public event Action<int> OnSkillPointsChanged;

    private void Awake()
    {
        ResetProgress();
    }

    public void ResetProgress()
    {
        currentLevel = Mathf.Max(1, startLevel);
        currentXP = Mathf.Max(0, startXP);
        unspentSkillPoints = Mathf.Max(0, startSkillPoints);
        OnSkillPointsChanged?.Invoke(unspentSkillPoints);
    }

    public int GetXPRequiredForNextLevel()
    {
        return currentLevel * 100;
    }
    
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= GetXPRequiredForNextLevel())
        {
            currentXP -= GetXPRequiredForNextLevel();
            currentLevel++;
            unspentSkillPoints++;
            OnSkillPointsChanged?.Invoke(unspentSkillPoints);
        }
    }
    
    public void AddXPScaled(int baseAmount, float extraMultiplier = 1f)
    {
        if (baseAmount <= 0) return;

        float levelMult = 1f + Mathf.Max(0, (currentLevel - 1)) * Mathf.Max(0f, xpGainBonusPerLevel);

        float statMult = 1f;
        if (useXPGainStat && statsManager != null)
        {
            // Erwartung: XPGain ist ein Multiplikator (1.0 = normal, 1.25 = +25%)
            statMult = Mathf.Max(0f, statsManager.GetValue(CoreStatId.XPGain));
        }

        float finalMult = Mathf.Max(0f, extraMultiplier) * levelMult * statMult;
        int finalXP = Mathf.RoundToInt(baseAmount * finalMult);

        AddXP(finalXP);
    }
    
    public void AddXPMultiplied(int baseAmount, float multiplier)
    {
        AddXPScaled(baseAmount, multiplier);
    }

    public bool SpendSkillPoints(int amount)
    {
        if (amount <= 0) return true;
        if (unspentSkillPoints < amount) return false;

        unspentSkillPoints -= amount;
        OnSkillPointsChanged?.Invoke(unspentSkillPoints);
        return true;
    }
}









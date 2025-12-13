using System;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    [Header("Startwerte für einen Run")]
    [SerializeField] private int startLevel = 1;
    [SerializeField] private int startXP = 0;
    [SerializeField] private int startSkillPoints = 0;

    private int currentLevel;
    private int currentXP;
    private int unspentSkillPoints;

    public int Level => currentLevel;
    public int XP => currentXP;
    public int SkillPoints => unspentSkillPoints;

    // NEU: Event
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

            // NEU: Event feuern (nach jeder Erhöhung)
            OnSkillPointsChanged?.Invoke(unspentSkillPoints);
        }
    }

    public void AddXPMultiplied(int baseAmount, float multiplier)
    {
        AddXP(Mathf.RoundToInt(baseAmount * Mathf.Max(0f, multiplier)));
    }

    public bool SpendSkillPoints(int amount)
    {
        if (amount <= 0) return true;
        if (unspentSkillPoints < amount) return false;

        unspentSkillPoints -= amount;

        // NEU: Event feuern (nach Ausgeben)
        OnSkillPointsChanged?.Invoke(unspentSkillPoints);

        return true;
    }
}








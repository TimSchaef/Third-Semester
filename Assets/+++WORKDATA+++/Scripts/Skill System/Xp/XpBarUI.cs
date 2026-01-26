using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XpBarUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerProgress progress;
    public Slider xpSlider;          
    public TMP_Text levelText;       
    public TMP_Text xpText;          

    void Awake()
    {
        if (!xpSlider)
            xpSlider = GetComponent<Slider>();
    }

    void Update()
    {
        if (!progress || !xpSlider) return;

        int currentXP = progress.XP;
        int needed = progress.GetXPRequiredForNextLevel();

        xpSlider.minValue = 0;
        xpSlider.maxValue = needed;
        xpSlider.value = currentXP;

        if (levelText)
            levelText.text = $"Lv {progress.Level}";

        if (xpText)
            xpText.text = $"{currentXP} / {needed}";
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string sceneToLoad;

    [Header("References")]
    [SerializeField] private GameObject winPanel; 

    [Header("Resume Target")]
    [SerializeField] private LevelUpSkillChoiceController levelUpSkillChoiceController;

    private void Awake()
    {
        if (winPanel == null) winPanel = gameObject;
    }

    
    public void Show()
    {
        Time.timeScale = 0f;
        winPanel.SetActive(true);
    }

    public void OnContinuePressed()
    {
        
        if (levelUpSkillChoiceController != null)
            levelUpSkillChoiceController.ResumeAfterWin();

        Time.timeScale = 1f;
        winPanel.SetActive(false);
    }

    public void OnChangeScenePressed()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogWarning("WinPanelUI: Keine Scene angegeben!");
    }
}





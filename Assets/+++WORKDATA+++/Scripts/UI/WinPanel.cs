using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Scene die beim Klick geladen wird (z.B. MainMenu)")]
    [SerializeField] private string sceneToLoad;
    
    public void OnContinuePressed()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
    
    public void OnChangeScenePressed()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("WinPanelUI: Keine Scene angegeben!");
        }
    }
}


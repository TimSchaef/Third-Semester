using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Scene die beim Klick geladen wird (z.B. MainMenu)")]
    [SerializeField] private string sceneToLoad;

    /// <summary>
    /// Button: Spiel fortsetzen (z.B. Endless / Sandbox)
    /// </summary>
    public void OnContinuePressed()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Button: Andere Scene laden
    /// </summary>
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


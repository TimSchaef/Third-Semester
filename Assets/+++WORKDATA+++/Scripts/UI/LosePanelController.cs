using UnityEngine;
using UnityEngine.SceneManagement;

public class LosePanelController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Pause / Cursor")]
    [SerializeField] private bool pauseGameOnShow = true;

    [Header("Disable While Open (optional)")]
    [SerializeField] private MonoBehaviour[] disableWhenOpen;

    [Header("Scene Buttons")]
    [Tooltip("Wenn leer, wird beim Retry die aktive Scene neu geladen.")]
    [SerializeField] private string retrySceneName = "";

    [Tooltip("Scene für Main Menu (muss in Build Settings sein).")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isOpen;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        panelRoot.SetActive(false);
        isOpen = false;
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;

        panelRoot.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseGameOnShow)
            Time.timeScale = 0f;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = false;
        }
    }

    public void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        panelRoot.SetActive(false);

        if (pauseGameOnShow)
            Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (disableWhenOpen != null)
        {
            foreach (var comp in disableWhenOpen)
                if (comp != null) comp.enabled = true;
        }
    }

    // --- Button hooks ---

    public void OnRetryPressed()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(retrySceneName))
            SceneManager.LoadScene(retrySceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("LosePanelController: mainMenuSceneName ist leer!");
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}

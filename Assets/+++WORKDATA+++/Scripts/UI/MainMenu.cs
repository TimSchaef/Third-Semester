using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject creditsPanel;
    
    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;
    
    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    
    void Start()
    {
        MusicManager.Instance.PlayMusic("MainMenu");
        
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettingsPanel);
        creditsButton.onClick.AddListener(OpenCreditsPanel);
        exitButton.onClick.AddListener(ExitGame);
        
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSliders(masterSlider, musicSlider, sfxSlider);
        }
    }
    
    
    private void StartGame()
    {
        SceneManager.LoadScene("Game-Random Skills");
        MusicManager.Instance.PlayMusic("Game");
    }
    
    private void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
    }

    private void OpenCreditsPanel()
    {
        creditsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    
    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void CloseCreditsPanel()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    

    private void ExitGame()
    {
        if (Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else            
            Application.Quit();
#endif            
        }
    }
   
}

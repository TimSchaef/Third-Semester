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
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        LoadVolume();
        
    }

    public void UpdateMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }
    
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }
    
    public void UpdateSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MasterVolume", out float masterVolume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
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

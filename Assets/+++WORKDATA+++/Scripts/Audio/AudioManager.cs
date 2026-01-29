using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    private void Awake()
    {
        if  (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadVolume();
    }
    
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(volume,0.0001f)) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(volume,0.0001f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(volume,0.0001f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    private void LoadVolume()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume= PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume= PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (masterSlider != null) masterSlider.value = masterVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
        
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    public void UpdateSliders(Slider master,  Slider music, Slider sfx)
    {
        masterSlider = master;
        musicSlider = music;
        sfxSlider = sfx;
        
        if (master != null) master.onValueChanged.AddListener(SetMasterVolume);
        if (music != null) music.onValueChanged.AddListener(SetMusicVolume);
        if (sfx != null) sfx.onValueChanged.AddListener(SetSFXVolume);
        
        LoadVolume();
    }
}

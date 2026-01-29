using UnityEngine;
using UnityEngine.UI;

public class GameSceneAudio : MonoBehaviour
{
    [Header("Audio Sliders in Game Scene")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    
    void Start()
    {
        // Connects the game scenes sliders to the persistent AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSliders(masterSlider, musicSlider, sfxSlider);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found in game scene!");
        }
    }
}

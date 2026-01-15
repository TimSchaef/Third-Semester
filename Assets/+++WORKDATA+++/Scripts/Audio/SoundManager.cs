using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundLibrary sfxLibrary;
    [SerializeField] private AudioSource sfx2DSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }

    public void PlaySound3D(string SoundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(SoundName),pos);
    }

    public void PlaySound2D(string SoundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(SoundName));
    }

}

using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string TrackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] musicTracks;

    public AudioClip GetAudioClipFromName(string trackName)
    {
        foreach (var track in musicTracks)
        {
            if (track.TrackName == trackName)
            {
                return track.clip;
            }
        }
        return null;
    }

}

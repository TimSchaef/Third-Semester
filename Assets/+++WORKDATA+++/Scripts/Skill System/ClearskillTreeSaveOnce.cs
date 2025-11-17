using UnityEngine;

public class ClearSkillTreeSaveOnce : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteKey("skilltree_unlocked_v2");   // oder "skilltree_unlocked", je nach KEY
        PlayerPrefs.Save();
        Debug.Log("SkillTree save deleted.");
    }
}

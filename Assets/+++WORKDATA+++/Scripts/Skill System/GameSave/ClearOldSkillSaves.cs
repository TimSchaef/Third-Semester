using UnityEngine;

public class ClearOldSkillSaves : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteKey("skilltree_unlocked");
        PlayerPrefs.DeleteKey("skilltree_unlocked_v2");
        PlayerPrefs.Save();
        Debug.Log("Alte SkillTree-Saves gelöscht.");
    }
}

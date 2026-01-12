using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Set(string name, string value)
    {
        if (nameText) nameText.text = name;
        if (valueText) valueText.text = value;
    }
}


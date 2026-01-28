using TMPro;
using UnityEngine;

public class KillCounterUI: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string prefix = "Kills: ";

    private void OnEnable()
    {
        if (KillCounter.Instance == null) return;

        KillCounter.Instance.OnKillsChanged += HandleChanged;
        HandleChanged(KillCounter.Instance.Kills); // refresh immediately
    }

    private void OnDisable()
    {
        if (KillCounter.Instance == null) return;
        KillCounter.Instance.OnKillsChanged -= HandleChanged;
    }

    private void HandleChanged(int kills)
    {
        if (label != null)
            label.text = prefix + kills;
    }
}


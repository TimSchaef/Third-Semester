using System;
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    public int Kills { get; private set; }
    public event Action<int> OnKillsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddKill(int amount = 1)
    {
        if (amount <= 0) return;
        Kills += amount;
        OnKillsChanged?.Invoke(Kills);
    }

    public void ResetKills()
    {
        Kills = 0;
        OnKillsChanged?.Invoke(Kills);
    }
}
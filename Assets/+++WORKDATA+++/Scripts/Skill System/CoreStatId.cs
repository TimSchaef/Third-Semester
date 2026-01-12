public enum CoreStatId
{
    MoveSpeed,       // Bewegungsgeschwindigkeit (Einheit: m/s Multiplikator oder absolut, siehe Definition)
    AttackSpeed,     // Angriffs-Geschwindigkeit (Multiplikator auf Cooldown/APS)
    MaxHP,           // Maximale Lebenspunkte
    HPRegen,         // Lebensregeneration pro Sekunde
    Armor,           // Rüstung (Damage-Reduction über Formel)
    XPGain,          // XP-Gewinn Multiplikator
    Thorns,          // Prozentualer Rückstoßschaden (0..1)
    LifeSteal,        // Lebensraub in % des verursachten Schadens (0..1)
    Damage,        // absoluter Schaden
    CritChance     // 0..1 (z.B. 0.25 = 25%)
}


namespace FoodDatabase.App.Models;

/// <summary>
/// Warnstufe für Verfallsdatum-Überwachung.
/// </summary>
public enum VerfallsdatumStatus
{
    /// <summary>
    /// Verfallsdatum ist mehr als 3 Tage entfernt.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Verfallsdatum läuft in den nächsten 3 Tagen ab (0–3 Tage).
    /// </summary>
    BaldAblaufend = 1,

    /// <summary>
    /// Verfallsdatum ist überschritten (negative Tage).
    /// </summary>
    Abgelaufen = 2
}

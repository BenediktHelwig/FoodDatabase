using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.App.Services.Interfaces;

/// <summary>
/// Service für Verfallsdatum-Warnungen (UC7).
/// Berechnet Warnstufen on-demand aus ProduktInstanz.Verfallsdatum.
/// </summary>
public interface IVerfallsdatumWarnungService
{
    /// <summary>
    /// Gibt alle Produkte mit kritischem Verfallsdatum (BaldAblaufend oder Abgelaufen) zurück.
    /// Sortiert nach Dringlichkeit: Abgelaufene zuerst, dann nach Datum aufsteigend.
    /// </summary>
    Task<List<VerfallsdatumWarnungDto>> GetWarnungenAsync();
}

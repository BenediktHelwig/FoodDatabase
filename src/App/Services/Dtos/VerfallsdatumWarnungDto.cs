using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Dtos;

/// <summary>
/// DTO für Verfallsdatum-Warnungen (UC7).
/// </summary>
public class VerfallsdatumWarnungDto
{
    /// <summary>
    /// ID der ProduktInstanz.
    /// </summary>
    public int ProduktInstanzId { get; set; }

    /// <summary>
    /// Name des Lebensmittels (aus LebensmittelKatalog).
    /// </summary>
    public string LebensmittelName { get; set; } = string.Empty;

    /// <summary>
    /// Verfallsdatum dieser Instanz.
    /// </summary>
    public DateTime Verfallsdatum { get; set; }

    /// <summary>
    /// Tage bis zum Ablauf. Negativ = bereits abgelaufen.
    /// </summary>
    public int TageBisAblauf { get; set; }

    /// <summary>
    /// Warnstufe (Ok, BaldAblaufend, Abgelaufen).
    /// </summary>
    public VerfallsdatumStatus Status { get; set; }
}

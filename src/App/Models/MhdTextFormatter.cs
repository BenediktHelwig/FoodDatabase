namespace FoodDatabase.App.Models;

/// <summary>
/// Formatiert Datumsangaben für die MHD-Anzeige in Tagen bis Verfallsdatum.
/// </summary>
public static class MhdTextFormatter
{
    /// <summary>
    /// Konvertiert die Anzahl der Tage bis zum Verfallsdatum in einen lokalisierten Text.
    /// Negative Werte zeigen die Tage seit Ablauf an.
    /// </summary>
    public static string GetMhdText(int tagesBisMhd)
    {
        if (tagesBisMhd < 0)
        {
            int tagedavor = Math.Abs(tagesBisMhd);
            return tagedavor == 1 ? "vor 1 Tag abgelaufen" : $"vor {tagedavor} Tagen abgelaufen";
        }
        else if (tagesBisMhd == 0)
        {
            return "Heute ablaufend";
        }
        else
        {
            return tagesBisMhd == 1 ? "1 Tag verbleibend" : $"{tagesBisMhd} Tage verbleibend";
        }
    }
}

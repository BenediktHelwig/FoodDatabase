namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für die Konvertierung von Einheiten zu Gramm.
    /// Wird für die Nährwert-Berechnung verwendet.
    /// </summary>
    public interface IEinheitConverter
    {
        /// <summary>
        /// Konvertiert eine Menge mit einer bestimmten Einheit zu Gramm.
        /// </summary>
        /// <param name="menge">Die Menge.</param>
        /// <param name="einheit">Die Einheit (z.B. "g", "ml", "TL", "EL").</param>
        /// <returns>Die konvertierte Menge in Gramm.</returns>
        double ConvertToGramm(double menge, string einheit);
    }
}

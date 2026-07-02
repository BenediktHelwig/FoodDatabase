using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Implementierung von IEinheitConverter.
    /// Konvertiert verschiedene Einheiten zu Gramm für standardisierte Nährwert-Berechnung.
    /// </summary>
    public class EinheitConverter : IEinheitConverter
    {
        /// <summary>
        /// Konvertiert eine Menge mit einer bestimmten Einheit zu Gramm.
        /// </summary>
        public double ConvertToGramm(double menge, string einheit)
        {
            return einheit.ToLower() switch
            {
                "g" => menge,                           // Gramm (direkt)
                "ml" => menge,                          // Milliliter (Wasser: 1ml ≈ 1g)
                "stück" => menge * 50,                  // Stück (durchschnittlich ~50g)
                "tl" => menge * 5,                      // Teelöffel (1 TL ≈ 5g)
                "el" => menge * 15,                     // Esslöffel (1 EL ≈ 15g)
                "tasse" => menge * 250,                 // Tasse (1 Tasse ≈ 250g)
                "prise" => menge * 0.5,                 // Prise (sehr wenig)
                _ => menge                              // Standard: als Gramm interpretieren
            };
        }
    }
}

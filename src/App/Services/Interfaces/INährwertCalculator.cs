using System.Threading.Tasks;
using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Nährwert-Berechnung für Rezepte (UC4 + UC5: Nährwertberechnung).
    /// Berechnet die Gesamt-Nährwerte und Nährwerte pro Portion basierend auf den Zutaten.
    /// </summary>
    public interface INährwertCalculator
    {
        /// <summary>
        /// Berechnet die Gesamt-Nährwerte und pro-Portion-Nährwerte für ein Rezept.
        /// Berücksichtigt die Menge jeder Zutat und deren Nährwertinformationen.
        /// </summary>
        Task<RezeptNährwerteDto> CalculateRezeptNährwerteAsync(int rezeptId);

        /// <summary>
        /// Berechnet die Nährwerte pro Portion für ein Rezept.
        /// </summary>
        Task<NährwerteProPortion> CalculateProPortionAsync(int rezeptId);
    }
}

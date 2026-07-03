using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Rezept-Nährwert-Dienste (UC5: Nährwert-Berechnung für Rezepte).
    /// Berechnet Gesamt- und Pro-Portion-Nährwerte für Rezepte.
    /// </summary>
    public interface IRezeptNährwertService
    {
        /// <summary>
        /// Ruft die Gesamt- und Pro-Portion-Nährwerte für ein Rezept ab.
        /// </summary>
        /// <param name="rezeptId">Die ID des Rezepts.</param>
        /// <returns>RezeptNährwerteDto mit Gesamt- und Pro-Portion-Nährwerten.</returns>
        Task<RezeptNährwerteDto> GetRezeptNährwerteAsync(int rezeptId);

        /// <summary>
        /// Berechnet Nährwerte direkt aus einer Liste von Zutaten.
        /// </summary>
        /// <param name="zutaten">Die Zutaten des Rezepts.</param>
        /// <param name="portionen">Die Anzahl der Portionen.</param>
        /// <returns>NährwerteDto mit den berechneten Nährwerten.</returns>
        Task<NährwerteDto> CalculateNährwerteFromZutatenAsync(IEnumerable<RezeptZutat> zutaten, int portionen);

        /// <summary>
        /// Passt die Nährwerte eines Rezepts für eine neue Portionszahl an.
        /// </summary>
        /// <param name="rezeptId">Die ID des Rezepts.</param>
        /// <param name="newPortionen">Die neue Portionszahl.</param>
        /// <returns>RezeptNährwerteDto mit angepassten Nährwerten.</returns>
        Task<RezeptNährwerteDto> AdjustNährwerteForPortionAsync(int rezeptId, int newPortionen);
    }
}

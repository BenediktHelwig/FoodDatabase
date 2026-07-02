using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Zutaten-Management innerhalb von Rezepten (UC4: Rezepte verwalten).
    /// Definiert CRUD-Operationen und Reordering für Zutaten.
    /// </summary>
    public interface IRezeptZutatService
    {
        /// <summary>
        /// Ruft alle Zutaten eines Rezepts ab, sortiert nach Position.
        /// </summary>
        Task<IEnumerable<RezeptZutat>> GetZutatenAsync(int rezeptId);

        /// <summary>
        /// Ruft eine einzelne Zutat ab.
        /// </summary>
        Task<RezeptZutat> GetZutatAsync(int rezeptId, int zutatId);

        /// <summary>
        /// Fügt eine neue Zutat zu einem Rezept hinzu.
        /// Die Position wird automatisch berechnet (nächste freie Position).
        /// </summary>
        Task<RezeptZutat> AddZutatAsync(int rezeptId, AddZutatRequest request);

        /// <summary>
        /// Aktualisiert eine bestehende Zutat.
        /// </summary>
        Task<RezeptZutat> UpdateZutatAsync(int rezeptId, int zutatId, UpdateZutatRequest request);

        /// <summary>
        /// Löscht eine Zutat aus einem Rezept.
        /// Ein Rezept muss mindestens 1 Zutat haben.
        /// </summary>
        Task DeleteZutatAsync(int rezeptId, int zutatId);

        /// <summary>
        /// Ordnet die Zutaten eines Rezepts neu.
        /// </summary>
        Task<IEnumerable<RezeptZutat>> ReorderZutatenAsync(int rezeptId, List<int> newOrder);

        /// <summary>
        /// Gibt die Anzahl der Zutaten eines Rezepts zurück.
        /// </summary>
        Task<int> GetZutatCountAsync(int rezeptId);
    }
}

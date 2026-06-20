using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Lebensmittel-Katalog-Verwaltung (UC1).
    /// Definiert CRUD-Operationen und Suchfunktionalität für Lebensmittel-Vorlagen.
    /// </summary>
    public interface ILebensmittelService
    {
        /// <summary>Erstellt ein neues Lebensmittel mit Validierung.</summary>
        Task<LebensmittelKatalog> CreateLebensmittelAsync(LebensmittelKatalog lebensmittel);

        /// <summary>Ruft ein Lebensmittel anhand seiner ID ab.</summary>
        Task<LebensmittelKatalog> GetLebensmittelByIdAsync(int id);

        /// <summary>Ruft alle Lebensmittel aus dem Katalog ab.</summary>
        Task<IEnumerable<LebensmittelKatalog>> GetAllLebensmittelAsync();

        /// <summary>Sucht Lebensmittel nach Name (case-insensitive).</summary>
        Task<IEnumerable<LebensmittelKatalog>> SearchLebensmittelAsync(string suchbegriff);

        /// <summary>Aktualisiert ein bestehendes Lebensmittel.</summary>
        Task<LebensmittelKatalog> UpdateLebensmittelAsync(LebensmittelKatalog lebensmittel);

        /// <summary>Löscht ein Lebensmittel anhand seiner ID.</summary>
        Task<bool> DeleteLebensmittelAsync(int id);
    }
}

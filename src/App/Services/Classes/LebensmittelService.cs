using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Verwaltung des Lebensmittel-Katalogs (UC1).
    /// Bietet CRUD-Operationen für Lebensmittel-Vorlagen,
    /// auf deren Basis ProduktInstanzen erstellt werden.
    /// </summary>
    public class LebensmittelService : ILebensmittelService
    {
        private readonly IRepository<LebensmittelKatalog> _repository;

        /// <summary>
        /// Initialisiert eine neue Instanz des LebensmittelService.
        /// </summary>
        /// <param name="repository">Repository für LebensmittelKatalog-Persistierung.</param>
        /// <exception cref="ArgumentNullException">Wenn repository null ist.</exception>
        public LebensmittelService(IRepository<LebensmittelKatalog> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Erstellt ein neues Lebensmittel im Katalog mit Validierungen.
        /// </summary>
        /// <param name="lebensmittel">Das zu erstellende Lebensmittel mit Name, Einheit und Kategorie.</param>
        /// <returns>Das neu erstellte Lebensmittel mit ID und Erstellungsdatum.</returns>
        /// <exception cref="ArgumentNullException">Wenn lebensmittel oder Name null/leer ist.</exception>
        /// <exception cref="ArgumentException">Wenn Name leer oder Einheit ungültig ist.</exception>
        public async Task<LebensmittelKatalog> CreateLebensmittelAsync(LebensmittelKatalog lebensmittel)
        {
            ValidateLebensmittel(lebensmittel);

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentException("Lebensmittel-Name darf nicht leer sein.");

            if (!IsValidEinheit(lebensmittel.Einheit))
                throw new ArgumentException($"Ungültige Einheit: {lebensmittel.Einheit}");

            lebensmittel.ErstelltAm = DateTime.UtcNow;
            return await _repository.AddAsync(lebensmittel);
        }

        /// <summary>
        /// Ruft ein Lebensmittel anhand seiner ID ab.
        /// </summary>
        /// <param name="id">Die ID des Lebensmittels. Muss > 0 sein.</param>
        /// <returns>Das gefundene Lebensmittel oder null, wenn nicht vorhanden.</returns>
        /// <exception cref="ArgumentException">Wenn ID <= 0.</exception>
        public async Task<LebensmittelKatalog> GetLebensmittelByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Ruft alle Lebensmittel aus dem Katalog ab.
        /// </summary>
        /// <returns>Enumerable aller Lebensmittel (kann leer sein).</returns>
        public async Task<IEnumerable<LebensmittelKatalog>> GetAllLebensmittelAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Sucht Lebensmittel nach Name (case-insensitive Substring-Suche).
        /// </summary>
        /// <param name="suchbegriff">Der Suchbegriff. Wenn null/leer, werden alle Lebensmittel zurückgegeben.</param>
        /// <returns>Liste aller Lebensmittel, deren Name den Suchbegriff enthält (case-insensitive).</returns>
        public async Task<IEnumerable<LebensmittelKatalog>> SearchLebensmittelAsync(string suchbegriff)
        {
            if (string.IsNullOrWhiteSpace(suchbegriff))
                return await _repository.GetAllAsync();

            IEnumerable<LebensmittelKatalog> alle = await _repository.GetAllAsync();
            // Case-insensitive substring search
            return alle.Where(l => l.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Aktualisiert ein bestehendes Lebensmittel mit Validierungen.
        /// </summary>
        /// <param name="lebensmittel">Das zu aktualisierendes Lebensmittel mit allen Feldern inkl. ID.</param>
        /// <returns>Das aktualisierte Lebensmittel.</returns>
        /// <exception cref="ArgumentNullException">Wenn lebensmittel oder Name null/leer ist.</exception>
        /// <exception cref="ArgumentException">Wenn ID <= 0, Name leer, oder Einheit ungültig ist.</exception>
        public async Task<LebensmittelKatalog> UpdateLebensmittelAsync(LebensmittelKatalog lebensmittel)
        {
            ValidateLebensmittel(lebensmittel);

            if (lebensmittel.Id <= 0)
                throw new ArgumentException("Ungültige ID.");

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentException("Lebensmittel-Name darf nicht leer sein.");

            if (!IsValidEinheit(lebensmittel.Einheit))
                throw new ArgumentException($"Ungültige Einheit: {lebensmittel.Einheit}");

            return await _repository.UpdateAsync(lebensmittel);
        }

        /// <summary>
        /// Löscht ein Lebensmittel aus dem Katalog.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Lebensmittels. Muss > 0 sein.</param>
        /// <returns>true, wenn erfolgreich gelöscht; false, wenn nicht gefunden.</returns>
        /// <exception cref="ArgumentException">Wenn ID <= 0.</exception>
        public async Task<bool> DeleteLebensmittelAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Prüft, ob ein Lebensmittel mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die ID des Lebensmittels.</param>
        /// <returns>true, wenn existiert; false sonst.</returns>
        public async Task<bool> LebensmittelExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            var lebensmittel = await _repository.GetByIdAsync(id);
            return lebensmittel is not null;
        }

        /// <summary>
        /// Validiert ein Lebensmittel-Objekt auf Null-Referenzen und wichtige Felder.
        /// Helper-Methode für Create/Update.
        /// </summary>
        /// <param name="lebensmittel">Das zu validierende Lebensmittel.</param>
        /// <exception cref="ArgumentNullException">Wenn lebensmittel oder Name null ist.</exception>
        private void ValidateLebensmittel(LebensmittelKatalog lebensmittel)
        {
            if (lebensmittel is null)
                throw new ArgumentNullException(nameof(lebensmittel));

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentNullException(nameof(lebensmittel.Name));
        }

        /// <summary>
        /// Prüft, ob eine Einheit gültig ist.
        /// Gültige Einheiten: "g" (Gramm), "ml" (Milliliter), "Stück" (Anzahl).
        /// Die Prüfung ist case-insensitive.
        /// </summary>
        /// <param name="einheit">Die zu prüfende Einheit.</param>
        /// <returns>true, wenn Einheit in der Gültig-Liste ist; false sonst.</returns>
        private bool IsValidEinheit(string einheit)
        {
            var validEinheiten = new[] { "g", "ml", "Stück" };
            return validEinheiten.Contains(einheit, StringComparer.OrdinalIgnoreCase);
        }
    }
}

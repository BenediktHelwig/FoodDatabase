using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Verwaltung von Produktinstanzen (UC10).
    /// Eine ProduktInstanz ist eine einzelne Packung eines Lebensmittels mit eigenem Verfallsdatum und Lagerort.
    /// </summary>
    public class ProduktInstanzService : IProduktInstanzService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        /// <summary>
        /// Initialisiert eine neue Instanz des ProduktInstanzService.
        /// </summary>
        /// <param name="repository">Repository für ProduktInstanz-Persistierung.</param>
        /// <exception cref="ArgumentNullException">Wenn repository null ist.</exception>
        public ProduktInstanzService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Erstellt eine neue ProduktInstanz (Packung) mit vollständiger Validierung.
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des Lebensmittels aus dem Katalog. Muss > 0 sein.</param>
        /// <param name="menge">Menge/Gewicht der Packung (in Gramm oder Einheit). Darf nicht negativ sein.</param>
        /// <param name="verfallsdatum">Mindesthaltbarkeitsdatum der Packung. Darf nicht in der Vergangenheit liegen.</param>
        /// <param name="lagerort">Lagerort-Konstante (z.B. Kühlschrank, Tiefkühler, Pantry). Muss valid sein.</param>
        /// <returns>Die neu erstellte ProduktInstanz mit ID, Einkaufsdatum und Erstellungsdatum.</returns>
        /// <exception cref="ArgumentException">Wenn Validierung fehlschlägt (LebensmittelId <= 0, Menge < 0, Verfallsdatum in der Vergangenheit, oder Lagerort ungültig).</exception>
        /// <exception cref="ArgumentNullException">Wenn Lagerort null oder leer ist.</exception>
        public async Task<ProduktInstanz> CreateAsync(int lebensmittelKatalogId, decimal menge, DateTime verfallsdatum, string lagerort)
        {
            // Validierungen: Alle Parameter werden überprüft, bevor Instanz erstellt wird
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein.");

            if (menge < 0)
                throw new ArgumentException("Menge darf nicht negativ sein.");

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.");

            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var produktInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = lebensmittelKatalogId,
                Menge = menge,
                Verfallsdatum = verfallsdatum,
                Einkaufsdatum = DateTime.UtcNow,
                Lagerort = lagerort,
                ErstelltAm = DateTime.UtcNow
            };

            return await _repository.AddAsync(produktInstanz);
        }

        /// <summary>
        /// Ruft eine ProduktInstanz anhand ihrer ID ab.
        /// </summary>
        /// <param name="id">Die ID der ProduktInstanz. Muss > 0 sein.</param>
        /// <returns>Die gefundene ProduktInstanz oder null, wenn nicht vorhanden.</returns>
        /// <exception cref="ArgumentException">Wenn ID <= 0.</exception>
        public async Task<ProduktInstanz> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Ruft alle ProduktInstanzen für ein bestimmtes Lebensmittel ab.
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des Lebensmittels aus dem Katalog. Muss > 0 sein.</param>
        /// <returns>Liste aller Instanzen dieses Lebensmittels (kann leer sein).</returns>
        /// <exception cref="ArgumentException">Wenn LebensmittelId <= 0.</exception>
        public async Task<List<ProduktInstanz>> GetByLebensmittelAsync(int lebensmittelKatalogId)
        {
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein.");

            var alle = await _repository.GetAllAsync();
            return alle
                .Where(p => p.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();
        }

        /// <summary>
        /// Ruft alle ProduktInstanzen für einen bestimmten Lagerort ab.
        /// </summary>
        /// <param name="lagerort">Der Lagerort (z.B. Kühlschrank). Muss valid und nicht leer sein.</param>
        /// <returns>Liste aller Instanzen an diesem Lagerort (kann leer sein).</returns>
        /// <exception cref="ArgumentNullException">Wenn Lagerort null oder leer ist.</exception>
        /// <exception cref="ArgumentException">Wenn Lagerort ungültig ist.</exception>
        public async Task<List<ProduktInstanz>> GetByLagerortAsync(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var alle = await _repository.GetAllAsync();
            return alle
                .Where(p => p.Lagerort == lagerort)
                .ToList();
        }

        /// <summary>
        /// Ruft alle ProduktInstanzen sortiert nach Verfallsdatum ab.
        /// Verwendet für FIFO-Logik: Älteste (früherste Verfallsdatum) zuerst.
        /// </summary>
        /// <returns>Liste aller Instanzen, sortiert nach Verfallsdatum (ascending).</returns>
        public async Task<List<ProduktInstanz>> GetNachVerfallsdatumSortiertAsync()
        {
            var alle = await _repository.GetAllAsync();
            // Sortierung ascending: Älteste (frühes Verfallsdatum) zuerst
            return alle
                .OrderBy(p => p.Verfallsdatum)
                .ToList();
        }

        /// <summary>
        /// Ruft alle abgelaufenen ProduktInstanzen ab.
        /// </summary>
        /// <param name="standdatum">Referenzdatum zum Vergleich. Wenn null, wird heute verwendet.</param>
        /// <returns>Liste aller Instanzen mit Verfallsdatum <= Standdatum.</returns>
        public async Task<List<ProduktInstanz>> GetVerfallenenAsync(DateTime? standdatum = null)
        {
            var referenzdatum = standdatum ?? DateTime.Today;
            var alle = await _repository.GetAllAsync();
            // Filter: Verfallsdatum liegt am oder vor dem Standdatum
            return alle
                .Where(p => p.Verfallsdatum <= referenzdatum)
                .ToList();
        }

        /// <summary>
        /// Aktualisiert eine bestehende ProduktInstanz mit neuen Werten.
        /// Alle Parameter werden validiert wie bei CreateAsync.
        /// </summary>
        /// <param name="id">ID der zu aktualisierenden ProduktInstanz. Muss > 0 sein.</param>
        /// <param name="menge">Neue Menge. Darf nicht negativ sein.</param>
        /// <param name="verfallsdatum">Neues Verfallsdatum. Darf nicht in der Vergangenheit liegen.</param>
        /// <param name="lagerort">Neuer Lagerort. Muss valid sein.</param>
        /// <exception cref="ArgumentException">Wenn Validierung fehlschlägt oder ID <= 0.</exception>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task UpdateAsync(int id, decimal menge, DateTime verfallsdatum, string lagerort)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            if (menge < 0)
                throw new ArgumentException("Menge darf nicht negativ sein.");

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.");

            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            instanz.Menge = menge;
            instanz.Verfallsdatum = verfallsdatum;
            instanz.Lagerort = lagerort;

            await _repository.UpdateAsync(instanz);
        }

        /// <summary>
        /// Löscht eine ProduktInstanz anhand ihrer ID.
        /// </summary>
        /// <param name="id">ID der zu löschenden ProduktInstanz. Muss > 0 sein.</param>
        /// <exception cref="ArgumentException">Wenn ID <= 0.</exception>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");
        }

        /// <summary>
        /// Berechnet die Anzahl der Tage bis zum Verfallsdatum einer ProduktInstanz.
        /// </summary>
        /// <param name="id">ID der ProduktInstanz. Muss > 0 sein.</param>
        /// <returns>Anzahl der Tage bis Verfallsdatum. Negative Werte = bereits abgelaufen.</returns>
        /// <exception cref="ArgumentException">Wenn ID <= 0.</exception>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task<int> GetTagesBisVerfallAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            var tage = (instanz.Verfallsdatum - DateTime.Today).Days;
            return tage;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Verwaltung des Lagerbestands (UC2).
    /// Aggregiert ProduktInstanzen und bietet Funktionen zur Bestandsverwaltung,
    /// Mindestbestand-Checks und Einkaufslisten-Generierung.
    /// </summary>
    public class LagerbestandService : ILagerbestandService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        /// <summary>
        /// Initialisiert eine neue Instanz des LagerbestandService.
        /// </summary>
        /// <param name="repository">Repository für ProduktInstanz-Persistierung.</param>
        public LagerbestandService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Berechnet die Gesamtmenge eines Lebensmittels über alle ProduktInstanzen hinweg.
        /// Summiert die Menge-Felder aller Packungen dieses Lebensmittels.
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des Lebensmittels aus dem Katalog.</param>
        /// <returns>Summe aller Mengen für dieses Lebensmittel (0 wenn keine Instanzen existieren).</returns>
        public async Task<decimal> GetGesamtmengeAsync(int lebensmittelKatalogId)
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                throw new NullReferenceException(nameof(allInstanzen));

            // Aggregation: Filtere nach LebensmittelId und summiere Menge
            return allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .Sum(x => x.Menge);
        }

        /// <summary>
        /// Prüft, ob der Mindestbestand für ein Lebensmittel unterschritten ist.
        /// Vergleicht: Gesamtmenge aller Instanzen < MindestbestandMenge einer Instanz
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des zu prüfenden Lebensmittels.</param>
        /// <returns>
        /// true, wenn: Keine Instanzen existieren ODER Gesamtmenge < Mindestbestand.
        /// false, wenn Gesamtmenge >= Mindestbestand.
        /// </returns>
        public async Task<bool> CheckMindestbestandUnterschrittenAsync(int lebensmittelKatalogId)
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null || !allInstanzen.Any())
                return true; // Kein Bestand = Mindestbestand unterschritten

            var instanzenFuerLebensmittel = allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();

            if (!instanzenFuerLebensmittel.Any())
                return true; // Keine Instanzen für dieses Lebensmittel = Mindestbestand unterschritten

            var gesamtmenge = instanzenFuerLebensmittel.Sum(x => x.Menge);
            // Hinweis: MindestbestandMenge sollte bei allen Instanzen desselben Lebensmittels gleich sein
            var mindestbestand = instanzenFuerLebensmittel.FirstOrDefault()?.MindestbestandMenge ?? 0;

            return gesamtmenge < mindestbestand;
        }

        /// <summary>
        /// Generiert eine Einkaufsliste für Lebensmittel, deren Bestand unter dem Mindestbestand liegt.
        /// Nutzt GroupBy zur Aggregation, um Mindestbestand-Unterschreitung zu erkennen.
        /// </summary>
        /// <returns>
        /// Liste von ProduktInstanzen, deren Lebensmittel einen Mindestbestand-Fehler haben.
        /// Gibt leere Liste zurück, wenn alle Bestände ok oder keine Instanzen existieren.
        /// </returns>
        public async Task<List<ProduktInstanz>> GetEinkaufslistenEintraegeAsync()
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                return new List<ProduktInstanz>();

            // GroupBy + Aggregation: Für jedes Lebensmittel, berechne Gesamtmenge und Mindestbestand
            var grouped = allInstanzen
                .GroupBy(x => x.LebensmittelKatalogId)
                .Select(g => new
                {
                    LebensmittelId = g.Key,
                    Gesamtmenge = g.Sum(x => x.Menge),
                    Mindestbestand = g.FirstOrDefault()?.MindestbestandMenge ?? 0,
                    Instanzen = g.ToList()
                })
                // Filter: Nur Lebensmittel mit Mindestbestand-Unterschreitung
                .Where(x => x.Gesamtmenge < x.Mindestbestand)
                // Flatten: Gebe alle Instanzen für die fehlerhaften Lebensmittel zurück
                .SelectMany(x => x.Instanzen)
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Ruft alle ProduktInstanzen für einen bestimmten Lagerort ab.
        /// Nützlich zur Anzeige des Lagerbestands nach Lagerort sortiert.
        /// </summary>
        /// <param name="lagerort">Der Lagerort-Konstante (z.B. Kühlschrank, Tiefkühler).</param>
        /// <returns>Liste aller Instanzen an diesem Lagerort (kann leer sein).</returns>
        /// <exception cref="ArgumentException">Wenn Lagerort null, leer oder ungültig ist.</exception>
        public async Task<List<ProduktInstanz>> GetBestaendePorLagerortAsync(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentException("Lagerort darf nicht null oder leer sein.", nameof(lagerort));

            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                return new List<ProduktInstanz>();

            // Filter: Alle Instanzen am gewünschten Lagerort
            return allInstanzen
                .Where(x => x.Lagerort == lagerort)
                .ToList();
        }

        /// <summary>
        /// Fügt eine neue ProduktInstanz (Packung) zum Bestand hinzu.
        /// Erstellt eine neue Instanz mit allen erforderlichen Feldern.
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des Lebensmittels aus dem Katalog.</param>
        /// <param name="menge">Menge der neuen Packung (muss > 0 sein).</param>
        /// <param name="mindestbestandMenge">Mindestbestand für dieses Lebensmittel.</param>
        /// <param name="verfallsdatum">Verfallsdatum der Packung (darf nicht in Vergangenheit liegen).</param>
        /// <param name="lagerort">Lagerort der Packung (z.B. Kühlschrank).</param>
        /// <returns>Die neu erstellte ProduktInstanz mit ID und Timestamps.</returns>
        /// <exception cref="ArgumentException">Wenn Menge <= 0 oder Verfallsdatum in der Vergangenheit.</exception>
        public async Task<ProduktInstanz> AddToBestandAsync(int lebensmittelKatalogId, decimal menge, decimal mindestbestandMenge, DateTime verfallsdatum, string lagerort)
        {
            if (menge <= 0)
                throw new ArgumentException("Menge muss größer als 0 sein.", nameof(menge));

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.", nameof(verfallsdatum));

            ProduktInstanz newInstanz = new()
            {
                LebensmittelKatalogId = lebensmittelKatalogId,
                Menge = menge,
                MindestbestandMenge = mindestbestandMenge,
                Verfallsdatum = verfallsdatum,
                Einkaufsdatum = DateTime.Today,
                Lagerort = lagerort,
                ErstelltAm = DateTime.UtcNow
            };

            return await _repository.AddAsync(newInstanz);
        }

        /// <summary>
        /// Aktualisiert die Menge einer bestehenden ProduktInstanz.
        /// </summary>
        /// <param name="id">ID der zu aktualisierenden ProduktInstanz. Muss > 0 sein.</param>
        /// <param name="newMenge">Neue Menge (muss > 0 sein).</param>
        /// <returns>Die aktualisierte ProduktInstanz.</returns>
        /// <exception cref="ArgumentException">Wenn newMenge <= 0.</exception>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task<ProduktInstanz> UpdateMengeAsync(int id, decimal newMenge)
        {
            if (newMenge <= 0)
                throw new ArgumentException("Menge muss größer als 0 sein.", nameof(newMenge));

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            instanz.Menge = newMenge;
            return await _repository.UpdateAsync(instanz);
        }

        /// <summary>
        /// Entfernt eine ProduktInstanz aus dem Bestand (Löschung).
        /// </summary>
        /// <param name="id">ID der zu löschenden ProduktInstanz.</param>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task RemoveFromBestandAsync(int id)
        {
            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Validiert, ob ausreichend Bestand für eine benötigte Menge vorhanden ist.
        /// Wird vor Verbrauch/Rezept-Zubereitung verwendet.
        /// </summary>
        /// <param name="id">ID der zu prüfenden ProduktInstanz.</param>
        /// <param name="requiredMenge">Benötigte Menge (z.B. für Rezept).</param>
        /// <returns>true, wenn Menge >= requiredMenge; false sonst.</returns>
        /// <exception cref="KeyNotFoundException">Wenn ProduktInstanz mit dieser ID nicht existiert.</exception>
        public async Task<bool> ValidateBestandAsync(int id, decimal requiredMenge)
        {
            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            return instanz.Menge >= requiredMenge;
        }
    }
}

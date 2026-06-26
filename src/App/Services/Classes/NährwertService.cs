using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Nährwertverwalтung (UC3).
    /// Implementiert CRUD-Operationen und Validierungen für Nährwerte.
    /// TDD Green-Phase: Implementierung folgt nach Test-Validierung durch Review-Agent.
    /// </summary>
    public class NährwertService : INährwertService
    {
        private readonly IRepository<Nährwert> _repository;
        private readonly IRepository<LebensmittelKatalog> _lebensmittelRepository;

        /// <summary>Initialisiert eine neue Instanz des NährwertService.</summary>
        /// <param name="repository">Repository für Nährwert-Zugriff.</param>
        /// <param name="lebensmittelRepository">Repository für LebensmittelKatalog-Zugriff.</param>
        /// <summary>
    /// Initialisiert eine neue Instanz des NährwertService.
    /// </summary>
    /// <param name="repository">Repository für Nährwert-Persistierung.</param>
    /// <param name="lebensmittelRepository">Repository für LebensmittelKatalog-Lookups.</param>
    public NährwertService(IRepository<Nährwert> repository, IRepository<LebensmittelKatalog> lebensmittelRepository)
    {
        _repository = repository;
        _lebensmittelRepository = lebensmittelRepository;
    }

    /// <summary>
    /// Ruft den Nährwert für ein Lebensmittel ab (nur nicht-archiviert).
    /// </summary>
    /// <param name="lebensmittelId">Die ID des Lebensmittels.</param>
    /// <returns>Der Nährwert oder null, wenn nicht vorhanden oder archiviert.</returns>
    public async Task<Nährwert> GetNährwertByLebensmittelIdAsync(int lebensmittelId)
    {
        var alleNährwerte = await _repository.GetAllAsync();
        return alleNährwerte.FirstOrDefault(n => n.LebensmittelId == lebensmittelId && !n.IsArchived);
    }

    /// <summary>
    /// Ruft alle nicht-archivierten Nährwerte ab, sortiert nach LebensmittelId.
    /// </summary>
    /// <returns>Enumerable aller aktiven Nährwerte.</returns>
    public async Task<IEnumerable<Nährwert>> GetAllNährwerteAsync()
    {
        var alleNährwerte = await _repository.GetAllAsync();
        return alleNährwerte.Where(n => !n.IsArchived).OrderBy(n => n.LebensmittelId);
    }

    /// <summary>
    /// Erstellt einen neuen Nährwert-Eintrag mit vollständiger Validierung.
    /// Prüfung erfolgt in dieser Reihenfolge: Duplikate → LebensmittelId → StandardMengeEinheit → Kalorien → Lebensmittel-Existenz.
    /// </summary>
    /// <param name="nährwert">Der zu erstellende Nährwert.</param>
    /// <returns>Der erstellte Nährwert mit gesetzter ID.</returns>
    /// <exception cref="InvalidOperationException">Wenn bereits ein Nährwert für dieses Lebensmittel existiert (UNIQUE Constraint).</exception>
    /// <exception cref="ArgumentException">Bei ungültigem LebensmittelId, StandardMengeEinheit, Kalorien oder nicht existierendem Lebensmittel.</exception>
    public async Task<Nährwert> CreateNährwertAsync(Nährwert nährwert)
    {
        // ZUERST: Prüfe Duplikate (UNIQUE Constraint auf LebensmittelId)
        var alleNährwerte = await _repository.GetAllAsync();
        if (alleNährwerte.Any(n => n.LebensmittelId == nährwert.LebensmittelId))
            throw new InvalidOperationException($"Es existiert bereits ein Nährwert für LebensmittelId {nährwert.LebensmittelId}.");

        // Validiere LebensmittelId
        if (nährwert.LebensmittelId <= 0)
            throw new ArgumentException("LebensmittelId muss größer als 0 sein.", nameof(nährwert.LebensmittelId));

        // Validiere StandardMengeEinheit
        if (!await ValidateStandardMengeEinheitAsync(nährwert.StandardMengeEinheit))
            throw new ArgumentException("StandardMengeEinheit muss 'g' oder 'ml' sein.", nameof(nährwert.StandardMengeEinheit));

        // Validiere Kalorien
        if (nährwert.Kalorien < 0)
            throw new ArgumentException("Kalorien dürfen nicht negativ sein.", nameof(nährwert.Kalorien));

        // Prüfe Lebensmittel-Existenz
        var alleLebensmittel = await _lebensmittelRepository.GetAllAsync();
        if (!alleLebensmittel.Any(l => l.Id == nährwert.LebensmittelId))
            throw new ArgumentException("Lebensmittel mit dieser ID existiert nicht.", nameof(nährwert.LebensmittelId));

        return await _repository.AddAsync(nährwert);
    }

    /// <summary>
    /// Aktualisiert einen Nährwert-Eintrag mit Validierung.
    /// </summary>
    /// <param name="nährwert">Der zu aktualisierende Nährwert.</param>
    /// <returns>Der aktualisierte Nährwert.</returns>
    /// <exception cref="ArgumentException">Wenn StandardMengeEinheit ungültig ist.</exception>
    public async Task<Nährwert> UpdateNährwertAsync(Nährwert nährwert)
    {
        // Validiere StandardMengeEinheit
        if (!await ValidateStandardMengeEinheitAsync(nährwert.StandardMengeEinheit))
            throw new ArgumentException("StandardMengeEinheit muss 'g' oder 'ml' sein.", nameof(nährwert.StandardMengeEinheit));

        return await _repository.UpdateAsync(nährwert);
    }

    /// <summary>
    /// Löscht einen Nährwert-Eintrag via Soft-Delete (IsArchived Flag).
    /// </summary>
    /// <param name="nährwertId">Die ID des zu löschenden Nährwerts.</param>
    /// <returns>Der archivierte Nährwert.</returns>
    public async Task<Nährwert> DeleteNährwertAsync(int nährwertId)
    {
        // Soft-Delete: Erstelle einen Nährwert mit der ID und setze IsArchived=true
        var nährwert = new Nährwert { Id = nährwertId, IsArchived = true };
        return await _repository.UpdateAsync(nährwert);
    }

    /// <summary>
    /// Validiert, dass StandardMengeEinheit nur "g" (Gramm) oder "ml" (Milliliter) sein darf.
    /// </summary>
    /// <param name="einheit">Die zu validierende Einheit.</param>
    /// <returns>true wenn gültig ("g" oder "ml"), false sonst.</returns>
    public async Task<bool> ValidateStandardMengeEinheitAsync(string einheit)
    {
        // Nur "g" oder "ml" erlaubt
        return await Task.FromResult(einheit is "g" or "ml");
    }
    }
}

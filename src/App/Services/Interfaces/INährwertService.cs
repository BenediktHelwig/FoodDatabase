using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Service-Interface für Nährwertverwalтung (UC3).
    /// Definiert CRUD-Operationen und Validierungen für Nährwerte.
    /// </summary>
    public interface INährwertService
    {
        /// <summary>
        /// Ruft den Nährwert für ein bestimmtes Lebensmittel ab.
        /// </summary>
        /// <param name="lebensmittelId">Die ID des Lebensmittels.</param>
        /// <returns>Der Nährwert, oder null wenn nicht vorhanden oder archiviert.</returns>
        Task<Nährwert> GetNährwertByLebensmittelIdAsync(int lebensmittelId);

        /// <summary>
        /// Ruft alle nicht archivierten Nährwerte ab.
        /// </summary>
        /// <returns>Eine Aufzählung aller aktiven Nährwerte.</returns>
        Task<IEnumerable<Nährwert>> GetAllNährwerteAsync();

        /// <summary>
        /// Erstellt einen neuen Nährwert-Eintrag.
        /// </summary>
        /// <param name="nährwert">Der zu erstellende Nährwert (muss ValidateStandardMengeEinheit passieren).</param>
        /// <returns>Der erstellte Nährwert mit gesetzter ID.</returns>
        /// <exception cref="ArgumentException">Wenn Validierung fehlschlägt oder Lebensmittel nicht existiert.</exception>
        /// <exception cref="InvalidOperationException">Wenn bereits ein Nährwert für dieses Lebensmittel existiert.</exception>
        Task<Nährwert> CreateNährwertAsync(Nährwert nährwert);

        /// <summary>
        /// Aktualisiert einen bestehenden Nährwert-Eintrag.
        /// </summary>
        /// <param name="nährwert">Der zu aktualisierende Nährwert (muss ValidateStandardMengeEinheit passieren).</param>
        /// <returns>Der aktualisierte Nährwert.</returns>
        /// <exception cref="ArgumentException">Wenn Validierung fehlschlägt.</exception>
        Task<Nährwert> UpdateNährwertAsync(Nährwert nährwert);

        /// <summary>
        /// Löscht einen Nährwert-Eintrag (Soft-Delete via IsArchived Flag).
        /// </summary>
        /// <param name="nährwertId">Die ID des zu löschenden Nährwerts.</param>
        /// <returns>Der archivierte Nährwert.</returns>
        Task<Nährwert> DeleteNährwertAsync(int nährwertId);

        /// <summary>
        /// Validiert, dass StandardMengeEinheit nur "g" oder "ml" sein darf.
        /// </summary>
        /// <param name="einheit">Die zu validierende Einheit.</param>
        /// <returns>true wenn gültig, false sonst.</returns>
        Task<bool> ValidateStandardMengeEinheitAsync(string einheit);
    }
}

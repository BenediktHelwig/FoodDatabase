using FoodDatabase.App.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Service-Interface für Lagerverwaltung (UC9).
    /// Verwaltet dynamische Lagerorte mit Validierung, Normalisierung und Auto-Complete.
    /// </summary>
    public interface ILagerortService
    {
        /// <summary>
        /// Ruft alle nicht archivierten Lagerorte ab.
        /// </summary>
        /// <returns>Enumerable aller aktiven Lagerorte.</returns>
        Task<IEnumerable<Lagerort>> GetAlleLagerorte();

        /// <summary>
        /// Sucht Lagerorte mit Fuzzy-Matching (Partial Prefix, Case-Insensitive).
        /// Wird für Auto-Complete-Funktionalität verwendet.
        /// </summary>
        /// <param name="prefix">Suchprefix (z.B. "lag" findet "Lager", "LagerA").</param>
        /// <returns>Lagerorte, die dem Prefix entsprechen.</returns>
        Task<IEnumerable<Lagerort>> GetLagerorteMitAutoComplete(string prefix);

        /// <summary>
        /// Validiert einen Lagornamen (nur Buchstaben A-Z, a-z).
        /// </summary>
        /// <param name="name">Der zu validierende Name.</param>
        /// <exception cref="ArgumentNullException">Wenn Name null oder leer ist.</exception>
        /// <exception cref="ArgumentException">Wenn Name ungültige Zeichen enthält (nicht nur Buchstaben).</exception>
        void ValidateLagerort(string name);

        /// <summary>
        /// Normalisiert einen Lagornamen (Single-Word → Capitalize: "lagerA" → "LagerA", "LAGER" → "Lager").
        /// </summary>
        /// <param name="input">Der zu normalisierende Input.</param>
        /// <returns>Der normalisierte Name (First Letter Upper, Rest Lower).</returns>
        string NormalisiereLagerort(string input);

        /// <summary>
        /// Erstellt einen neuen Lagerort oder gibt den existierenden zurück (GetOrCreate-Pattern).
        /// Normalisiert den Namen vor Verarbeitung. Verhindert Duplikate durch DB UNIQUE Index.
        /// </summary>
        /// <param name="name">Der Name des Lagerorts (wird normalisiert).</param>
        /// <returns>Der neu erstellte oder bestehende Lagerort.</returns>
        /// <exception cref="ArgumentException">Wenn der Name ungültig ist (nicht nur Buchstaben).</exception>
        Task<Lagerort> GetOrCreateAsync(string name);
    }
}

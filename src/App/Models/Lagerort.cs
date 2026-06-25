using System;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert einen Lagerort im Lagerverwaltungssystem.
    /// Lagerorte werden dynamisch erstellt (UC9) und können verschiedene Lagerbereiche sein
    /// (z.B. "Kühlschrank", "Tiefkühler", "Pantry", "KuehlraumEins", etc.).
    /// </summary>
    public class Lagerort
    {
        /// <summary>Eindeutige ID des Lagerorts.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Name des Lagerorts (UNIQUE, Case-Sensitive).
        /// Wird normalisiert: Single-Word-Input → Capitalize (z.B. "lagerA" → "LagerA", "LAGER" → "Lager").
        /// Nur Buchstaben (A-Z, a-z) erlaubt nach Validierung.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Zeitstempel, wann dieser Lagerort im System erstellt wurde.
        /// Standard: UTC-Jetzt.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft-Delete Flag: Lagerorte werden nicht gelöscht, nur archiviert.
        /// Dies erhält das Audit-Trail und erlaubt historische Abfragen.
        /// </summary>
        public bool IsArchived { get; set; } = false;
    }
}

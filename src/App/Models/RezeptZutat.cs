using System;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert eine Zutat innerhalb eines Rezepts.
    /// Gehört zur Rezept-Aggregate und wird durch ON DELETE CASCADE gelöscht,
    /// wenn das übergeordnete Rezept gelöscht wird.
    /// </summary>
    public class RezeptZutat
    {
        /// <summary>Eindeutige ID dieser Zutat.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign Key zum übergeordneten Rezept.
        /// ON DELETE CASCADE: Wenn das Rezept gelöscht wird, werden auch die Zutaten gelöscht.
        /// </summary>
        public int RezeptId { get; set; }

        /// <summary>
        /// Foreign Key zum Lebensmittel aus dem Katalog.
        /// ON DELETE RESTRICT: Ein Lebensmittel kann nicht gelöscht werden, wenn es in Rezepten verwendet wird.
        /// </summary>
        public int LebensmittelId { get; set; }

        /// <summary>Navigation Property: Das übergeordnete Rezept.</summary>
        public Rezept Rezept { get; set; }

        /// <summary>Navigation Property: Das Lebensmittel aus dem Katalog.</summary>
        public LebensmittelKatalog Lebensmittel { get; set; }

        /// <summary>
        /// Menge der Zutat.
        /// Muss zwischen 0.01 und 10000 liegen (z.B. 0.5 TL oder 5000g).
        /// </summary>
        public double Menge { get; set; }

        /// <summary>
        /// Einheit der Menge.
        /// Erlaubte Werte: "g" (Gramm), "ml" (Milliliter), "Stück", "TL" (Teelöffel),
        /// "EL" (Esslöffel), "Tasse", "Prise".
        /// </summary>
        public string Einheit { get; set; }

        /// <summary>
        /// Optionale Notizen zur Zutat (z.B. "gehackt", "klein geschnitten").
        /// </summary>
        public string? Notizen { get; set; }

        /// <summary>
        /// Position dieser Zutat in der Zutatenliste (0-basiert, z.B. 0, 1, 2, ...).
        /// Wird für die Sortierung verwendet.
        /// Muss eindeutig pro Rezept sein (UNIQUE Constraint auf (RezeptId, Position)).
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Zeitstempel, wann diese Zutat zum Rezept hinzugefügt wurde (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

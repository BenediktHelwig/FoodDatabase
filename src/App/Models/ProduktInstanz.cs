using System;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert eine einzelne Produktinstanz (Packung) eines Lebensmittels.
    /// Jede Instanz hat ihr eigenes Verfallsdatum, Einkaufsdatum und Lagerort.
    /// </summary>
    public class ProduktInstanz
    {
        /// <summary>Eindeutige ID der Produktinstanz.</summary>
        public int Id { get; set; }

        /// <summary>Fremdschlüssel zum Lebensmittel-Katalog. Referenziert das Produkt-Template.</summary>
        public int LebensmittelKatalogId { get; set; }

        /// <summary>
        /// Menge der Packung (in der Einheit des Lebensmittels: g, ml, oder Stück).
        /// Wird beim Verbrauch reduziert oder bei Verbrauch = 0 gelöscht.
        /// </summary>
        public decimal Menge { get; set; }

        /// <summary>
        /// Mindestbestand für dieses Lebensmittel (Schwellenwert für Einkaufslisten-Trigger).
        /// Wird über alle Instanzen desselben Lebensmittels aggregiert.
        /// </summary>
        public decimal MindestbestandMenge { get; set; }

        /// <summary>
        /// Mindesthaltbarkeitsdatum dieser Packung.
        /// Wird für FIFO-Sortierung (älteste zuerst) und Verfalls-Warnungen verwendet.
        /// </summary>
        public DateTime Verfallsdatum { get; set; }

        /// <summary>
        /// Kaufdatum dieser Packung (Zeitpunkt der Lagerung).
        /// Zur Nachverfolgung von Bestandsalter und FIFO-Logik.
        /// </summary>
        public DateTime Einkaufsdatum { get; set; }

        /// <summary>
        /// Lagerort der Packung (Konstante: Kühlschrank, Tiefkühler, Pantry, Anderes).
        /// Wird für Bestandsverwaltung pro Lagerort und Raumabfrage verwendet.
        /// </summary>
        public string Lagerort { get; set; }

        /// <summary>
        /// Zeitstempel, wann diese Instanz im System erstellt wurde.
        /// Standard: UTC-Jetzt.
        /// </summary>
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        /// <summary>Navigation property: Referenz zum LebensmittelKatalog-Eintrag.</summary>
        public LebensmittelKatalog LebensmittelKatalog { get; set; }
    }
}

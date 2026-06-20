using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert einen Eintrag im Lebensmittel-Katalog.
    /// Der Katalog ist eine Vorlage/Master-List für alle Lebensmittel im System.
    /// Für jedes Katalog-Einträge können mehrere ProduktInstanzen mit unterschiedlichen Verfallsdaten existieren.
    /// </summary>
    public class LebensmittelKatalog
    {
        /// <summary>Eindeutige ID des Katalog-Eintrags.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Name des Lebensmittels (z.B. "Mehl", "Milch", "Tomaten").
        /// Muss eindeutig und nicht leer sein.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Einheit für Menge-Angaben: "g" (Gramm), "ml" (Milliliter), "Stück" (Anzahl).
        /// Wird verwendet, um Mengen von ProduktInstanzen korrekt zu interpretieren.
        /// </summary>
        public string Einheit { get; set; }

        /// <summary>
        /// Kategorie des Lebensmittels für Organisations-Zwecke (z.B. "Mehl", "Milchprodukte", "Obst").
        /// Optional; wird später für Filter und Navigation genutzt.
        /// </summary>
        public string Kategorie { get; set; }

        /// <summary>
        /// Zeitstempel, wann dieser Katalog-Eintrag erstellt wurde.
        /// Standard: UTC-Jetzt.
        /// </summary>
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        /// <summary>Navigation property: Sammlung aller ProduktInstanzen dieses Lebensmittels (wird später hinzugefügt).</summary>
        // public ICollection<ProduktInstanz> ProduktInstanzen { get; set; }
    }
}

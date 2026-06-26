using System;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert Nährwertinformationen für ein Lebensmittel (UC3).
    /// Alle Nährwertangaben beziehen sich auf eine Standardmenge von 100 Einheiten (100g oder 100ml).
    /// </summary>
    public class Nährwert
    {
        /// <summary>Standardmenge, auf die sich alle Nährwertangaben beziehen. KONSTANT: 100.</summary>
        public const int STANDARD_MENGE_WERT = 100;

        /// <summary>Eindeutige ID des Nährwert-Eintrags.</summary>
        public int Id { get; set; }

        /// <summary>
        /// ID des zugehörigen Lebensmittels (FK zu LebensmittelKatalog).
        /// Ein Lebensmittel hat genau einen Nährwert-Eintrag.
        /// </summary>
        public int LebensmittelId { get; set; }

        /// <summary>
        /// Energiegehalt in Kilokalorien (kcal) pro 100 Einheiten.
        /// IMMER ganzzahlig (int), keine Komma-Werte.
        /// </summary>
        public int Kalorien { get; set; }

        /// <summary>Fettgehalt pro 100 Einheiten. Komma-Werte möglich (z.B. 15,5).</summary>
        public double Fett { get; set; }

        /// <summary>
        /// Gesättigte Fettsäuren pro 100 Einheiten.
        /// Komma-Werte möglich (z.B. 8,2).
        /// </summary>
        public double GesättigteFettsäuren { get; set; }

        /// <summary>Kohlenhydratgehalt pro 100 Einheiten. Komma-Werte möglich (z.B. 45,3).</summary>
        public double Kohlenhydrate { get; set; }

        /// <summary>
        /// Zuckergehalt pro 100 Einheiten.
        /// Komma-Werte möglich (z.B. 22,5).
        /// </summary>
        public double Zucker { get; set; }

        /// <summary>
        /// Proteingehalt (Eiweiß) pro 100 Einheiten.
        /// Komma-Werte möglich. Kann 0 oder 0,xx sein (z.B. 0, 0,5, 8,3).
        /// </summary>
        public double Protein { get; set; }

        /// <summary>
        /// Ballaststoffgehalt pro 100 Einheiten.
        /// Komma-Werte möglich. Kann 0 oder 0,xx sein (z.B. 0, 2,1, 5,8).
        /// </summary>
        public double Ballaststoffe { get; set; }

        /// <summary>
        /// Salzgehalt (Natrium) pro 100 Einheiten.
        /// Komma-Werte möglich. Kann 0 oder 0,xx sein (z.B. 0, 0,05, 1,2).
        /// </summary>
        public double Salz { get; set; }

        /// <summary>
        /// Einheit der Standardportion: "g" (Gramm) oder "ml" (Milliliter).
        /// Wird bei der Erstellung ausgewählt und persistent gespeichert.
        /// Validierung (nur "g" oder "ml") gehört zur Businesslogik.
        /// Initial: null (erzwingt explizite Auswahl im UI).
        /// </summary>
        public string StandardMengeEinheit { get; set; }

        /// <summary>
        /// Zeitstempel, wann dieser Nährwert-Eintrag im System erstellt wurde.
        /// Standard: UTC-Jetzt.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft-Delete Flag: Nährwerte werden nicht gelöscht, nur archiviert.
        /// Dies erhält das Audit-Trail und erlaubt historische Abfragen.
        /// </summary>
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Navigation Property zu LebensmittelKatalog.
        /// Wird von EF Core verwaltet.
        /// </summary>
        public LebensmittelKatalog LebensmittelKatalog { get; set; }
    }
}

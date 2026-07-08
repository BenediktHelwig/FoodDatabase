namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>
    /// UC8: Einkaufslisten-Eintrag (DTO für Einkaufslistenservice).
    /// Repräsentiert ein Lebensmittel, das gekauft werden muss (Gesamtmenge <= Mindestbestand).
    /// Aggregiert alle ProduktInstanzen desselben LebensmittelKatalogId.
    /// </summary>
    public class EinkaufslistenEintragDto
    {
        /// <summary>Eindeutige ID des Lebensmittels im Katalog.</summary>
        public int LebensmittelKatalogId { get; set; }

        /// <summary>Name des Lebensmittels (aus LebensmittelKatalog.Name oder Fallback).</summary>
        public string LebensmittelName { get; set; }

        /// <summary>Aktuelle Gesamtmenge aller Instanzen dieses Lebensmittels.</summary>
        public decimal AktuelleGesamtmenge { get; set; }

        /// <summary>Mindestbestand für dieses Lebensmittel (aus First-Instanz).</summary>
        public decimal MindestbestandMenge { get; set; }

        /// <summary>Fehlmenge = MindestbestandMenge - AktuelleGesamtmenge.</summary>
        public decimal Fehlmenge { get; set; }

        /// <summary>Einheit des Lebensmittels (g, ml, Stück, etc. aus LebensmittelKatalog).</summary>
        public string Einheit { get; set; }
    }
}

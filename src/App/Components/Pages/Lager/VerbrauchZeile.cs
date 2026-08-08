namespace FoodDatabase.App.Components.Pages.Lager
{
    /// <summary>
    /// View-Model für eine Zeile in der Verbrauch-Ausbuchungs-Tabelle (UC6).
    /// Enthält aggregierte Bestandsinformationen eines Lebensmittels für die UI-Anzeige.
    /// </summary>
    public class VerbrauchZeile
    {
        /// <summary>
        /// Eindeutige ID des Lebensmittels aus dem Katalog.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name des Lebensmittels (z.B. "Milch", "Käse").
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Einheit des Lebensmittels (z.B. "l", "kg", "Stück").
        /// </summary>
        public string Einheit { get; set; } = "";

        /// <summary>
        /// Gesamtmenge aller Packungen dieses Lebensmittels im Bestand.
        /// Summe über alle ProduktInstanzen des Lebensmittels.
        /// </summary>
        public decimal Gesamtmenge { get; set; }

        /// <summary>
        /// Gibt an, ob der Mindestbestand für dieses Lebensmittel unterschritten ist.
        /// true = Gesamtmenge &lt; konfigurierter Mindestbestand → Badge-Anzeige in UI.
        /// </summary>
        public bool MindestbestandUnterschritten { get; set; }
    }
}

using System;

namespace FoodDatabase.App.Models
{
    public class ProduktInstanz
    {
        public int Id { get; set; }
        public int LebensmittelKatalogId { get; set; }
        public int LagerortId { get; set; }
        public double AktuelleM enge { get; set; }
        public double MindestbestandMenge { get; set; }
        public DateTime Verfallsdatum { get; set; }
        public DateTime Einkaufsdatum { get; set; }
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public LebensmittelKatalog LebensmittelKatalog { get; set; }
        public Lagerort Lagerort { get; set; }
    }
}

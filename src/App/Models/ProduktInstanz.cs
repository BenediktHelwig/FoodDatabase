using System;

namespace FoodDatabase.App.Models
{
    public class ProduktInstanz
    {
        public int Id { get; set; }
        public int LebensmittelKatalogId { get; set; }
        public decimal Menge { get; set; }
        public decimal MindestbestandMenge { get; set; }
        public DateTime Verfallsdatum { get; set; }
        public DateTime Einkaufsdatum { get; set; }
        public string Lagerort { get; set; }
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        // Navigation property
        public LebensmittelKatalog LebensmittelKatalog { get; set; }
    }
}

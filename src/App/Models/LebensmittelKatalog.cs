using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    public class LebensmittelKatalog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Einheit { get; set; } // g, ml, Stück
        public string Kategorie { get; set; } // Optional
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Nährwert> Nährwerte { get; set; } = new List<Nährwert>();
        public ICollection<ProduktInstanz> ProduktInstanzen { get; set; } = new List<ProduktInstanz>();
        public ICollection<RezeptZutat> RezeptZutaten { get; set; } = new List<RezeptZutat>();
    }
}

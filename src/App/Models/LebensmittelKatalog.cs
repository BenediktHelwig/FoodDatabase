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

        // Navigation properties (werden später hinzugefügt, wenn weitere Entities existieren)
    }
}

using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    public class Lagerort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ProduktInstanz> ProduktInstanzen { get; set; } = new List<ProduktInstanz>();
    }
}

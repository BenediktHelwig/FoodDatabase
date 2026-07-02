namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>Request für Rezept-Erstellung.</summary>
    public class CreateRezeptRequest
    {
        public string Name { get; set; }
        public string? Beschreibung { get; set; }
        public string? Zubereitung { get; set; }
        public int Portionen { get; set; }
        public int ZubereitungszeitMinuten { get; set; }
        public string Schwierigkeitsgrad { get; set; }
    }
}

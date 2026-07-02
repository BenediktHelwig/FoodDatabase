namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>Request für Zutat-Erstellung.</summary>
    public class AddZutatRequest
    {
        public int LebensmittelId { get; set; }
        public double Menge { get; set; }
        public string Einheit { get; set; }
        public string? Notizen { get; set; }
    }
}

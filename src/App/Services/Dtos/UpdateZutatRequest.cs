namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>Request für Zutat-Update.</summary>
    public class UpdateZutatRequest
    {
        public double? Menge { get; set; }
        public string? Einheit { get; set; }
        public string? Notizen { get; set; }
    }
}

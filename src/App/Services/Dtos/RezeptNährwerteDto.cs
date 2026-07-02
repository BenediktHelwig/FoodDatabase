namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>DTO für Rezept-Nährwerte (Gesamt + Pro Portion).</summary>
    public class RezeptNährwerteDto
    {
        public int RezeptId { get; set; }
        public NährwerteDto GesamtnährwerteDto { get; set; }
        public NährwerteDto ProPortionNährwerteDto { get; set; }
    }
}

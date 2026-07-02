namespace FoodDatabase.App.Services.Dtos
{
    /// <summary>DTO für Nährwerte-Details.</summary>
    public class NährwerteDto
    {
        public int Kalorien { get; set; }
        public double Fett { get; set; }
        public double GesättigteFettsäuren { get; set; }
        public double Kohlenhydrate { get; set; }
        public double Zucker { get; set; }
        public double Protein { get; set; }
        public double Ballaststoffe { get; set; }
        public double Salz { get; set; }
    }
}

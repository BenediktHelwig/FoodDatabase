using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    public static class LagerortKonstanten
    {
        public const string Kühlschrank = "Kühlschrank";
        public const string Tiefkühler = "Tiefkühler";
        public const string Pantry = "Pantry";
        public const string Anderes = "Anderes";

        public static readonly string[] AlleWerte = { Kühlschrank, Tiefkühler, Pantry, Anderes };

        public static bool IsValidLagerort(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                return false;

            return Array.Exists(AlleWerte, element => element.Equals(lagerort, StringComparison.OrdinalIgnoreCase));
        }
    }
}

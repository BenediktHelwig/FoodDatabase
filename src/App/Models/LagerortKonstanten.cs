using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Zentrale Konstanten für alle gültigen Lagerorte im System.
    /// Wird verwendet, um Lagerorte zu validieren und einzuschränken.
    /// </summary>
    public static class LagerortKonstanten
    {
        /// <summary>Kühlschrank – für temperatur-empfindliche Lebensmittel (4°C).</summary>
        public const string Kühlschrank = "Kühlschrank";

        /// <summary>Tiefkühler – für gefrorene Produkte (-18°C).</summary>
        public const string Tiefkühler = "Tiefkühler";

        /// <summary>Pantry – für trocken/ungekühlt lagernde Lebensmittel (Raumtemperatur).</summary>
        public const string Pantry = "Pantry";

        /// <summary>Anderes – für sonstige Lagerorte (flexibel definierbar).</summary>
        public const string Anderes = "Anderes";

        /// <summary>Array aller gültigen Lagerorte. Wird für Validierungen verwendet.</summary>
        public static readonly string[] AlleWerte = { Kühlschrank, Tiefkühler, Pantry, Anderes };

        /// <summary>
        /// Validiert, ob ein Lagerort-String gültig ist (case-insensitive).
        /// </summary>
        /// <param name="lagerort">Der zu validierende Lagerort.</param>
        /// <returns>true, wenn lagerort in AlleWerte enthalten ist (case-insensitive); false sonst.</returns>
        public static bool IsValidLagerort(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                return false;

            return Array.Exists(AlleWerte, element => element.Equals(lagerort, StringComparison.OrdinalIgnoreCase));
        }
    }
}

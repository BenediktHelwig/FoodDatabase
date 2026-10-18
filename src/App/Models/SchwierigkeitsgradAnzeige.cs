namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Zentrale Zuordnung der gespeicherten Schwierigkeitsgrade (Easy/Medium/Hard) zur deutschen Anzeige.
    /// Die gespeicherten Werte müssen mit RezeptService.ValidateSchwierigkeitsgrad übereinstimmen.
    /// </summary>
    public static class SchwierigkeitsgradAnzeige
    {
        /// <summary>Gespeicherter Wert für leicht.</summary>
        public const string Easy = "Easy";

        /// <summary>Gespeicherter Wert für mittel (Default).</summary>
        public const string Medium = "Medium";

        /// <summary>Gespeicherter Wert für schwer.</summary>
        public const string Hard = "Hard";

        /// <summary>Array aller gültigen Schwierigkeitsgrade, z.B. für Auswahllisten.</summary>
        public static readonly string[] AlleWerte = { Easy, Medium, Hard };

        /// <summary>
        /// Liefert die deutsche Bezeichnung eines Schwierigkeitsgrads.
        /// Unbekannte Werte werden unverändert zurückgegeben, null ergibt einen leeren String.
        /// </summary>
        /// <param name="wert">Der gespeicherte Schwierigkeitsgrad.</param>
        /// <returns>Leicht, Mittel oder Schwer; sonst der Eingabewert selbst.</returns>
        public static string ZuDeutsch(string wert)
        {
            if (wert is null)
            {
                return string.Empty;
            }

            return wert switch
            {
                Easy => "Leicht",
                Medium => "Mittel",
                Hard => "Schwer",
                _ => wert
            };
        }
    }
}

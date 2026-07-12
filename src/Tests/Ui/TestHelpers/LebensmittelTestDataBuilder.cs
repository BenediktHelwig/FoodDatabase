using FoodDatabase.App.Models;

namespace FoodDatabase.Tests.Ui.TestHelpers
{
    /// <summary>
    /// Builder-Klasse für Test-Daten der Lebensmittel und Nährwert-Modelle.
    /// Vereinfacht die Erstellung von Objekte mit sinnvollen Defaults in Tests.
    /// </summary>
    public class LebensmittelTestDataBuilder
    {
        private LebensmittelKatalog lebensmittel = new LebensmittelKatalog();
        private Nährwert nährwert = new Nährwert();

        /// <summary>Erstellt einen Lebensmittel Builder mit Default-Werten.</summary>
        public static LebensmittelTestDataBuilder CreateLebensmittel()
        {
            return new LebensmittelTestDataBuilder();
        }

        /// <summary>Erstellt einen Nährwert Builder mit Default-Werten.</summary>
        public static NährwertTestDataBuilder CreateNährwert()
        {
            return new NährwertTestDataBuilder();
        }

        /// <summary>Setzt die ID des Lebensmittels.</summary>
        public LebensmittelTestDataBuilder WithId(int id)
        {
            lebensmittel.Id = id;
            return this;
        }

        /// <summary>Setzt den Namen des Lebensmittels.</summary>
        public LebensmittelTestDataBuilder WithName(string name)
        {
            lebensmittel.Name = name;
            return this;
        }

        /// <summary>Setzt die Einheit des Lebensmittels (g, ml, Stück).</summary>
        public LebensmittelTestDataBuilder WithEinheit(string einheit)
        {
            lebensmittel.Einheit = einheit;
            return this;
        }

        /// <summary>Setzt die Kategorie des Lebensmittels.</summary>
        public LebensmittelTestDataBuilder WithKategorie(string kategorie)
        {
            lebensmittel.Kategorie = kategorie;
            return this;
        }

        /// <summary>Setzt das Erstellungs-Datum.</summary>
        public LebensmittelTestDataBuilder WithErstelltAm(DateTime datum)
        {
            lebensmittel.ErstelltAm = datum;
            return this;
        }

        /// <summary>Erstellt das Lebensmittel mit Standard-Werten (wenn nicht gesetzt).</summary>
        public LebensmittelKatalog Build()
        {
            if (string.IsNullOrEmpty(lebensmittel.Name))
                lebensmittel.Name = "Test Lebensmittel";

            if (string.IsNullOrEmpty(lebensmittel.Einheit))
                lebensmittel.Einheit = "g";

            if (string.IsNullOrEmpty(lebensmittel.Kategorie))
                lebensmittel.Kategorie = "Test Kategorie";

            return lebensmittel;
        }

        /// <summary>Erstellt eine Liste von mehreren Test-Lebensmitteln.</summary>
        public static List<LebensmittelKatalog> CreateTestLebensmittelList(int count = 3)
        {
            var list = new List<LebensmittelKatalog>();
            var kategorien = new[] { "Getreide", "Milchprodukte", "Obst", "Gemüse", "Fleisch" };
            var einheiten = new[] { "g", "ml", "Stück" };

            for (int i = 1; i <= count; i++)
            {
                list.Add(new LebensmittelKatalog
                {
                    Id = i,
                    Name = $"Lebensmittel {i}",
                    Einheit = einheiten[i % einheiten.Length],
                    Kategorie = kategorien[i % kategorien.Length],
                    ErstelltAm = DateTime.UtcNow.AddDays(-i)
                });
            }

            return list;
        }
    }

    /// <summary>
    /// Builder-Klasse für Test-Daten des Nährwert-Modells.
    /// </summary>
    public class NährwertTestDataBuilder
    {
        private Nährwert nährwert = new Nährwert();

        /// <summary>Setzt die ID des Nährwerts.</summary>
        public NährwertTestDataBuilder WithId(int id)
        {
            nährwert.Id = id;
            return this;
        }

        /// <summary>Setzt die zugehörige Lebensmittel-ID.</summary>
        public NährwertTestDataBuilder WithLebensmittelId(int lebensmittelId)
        {
            nährwert.LebensmittelId = lebensmittelId;
            return this;
        }

        /// <summary>Setzt den Kaloriengehalt (ganzzahlig).</summary>
        public NährwertTestDataBuilder WithKalorien(int kalorien)
        {
            nährwert.Kalorien = kalorien;
            return this;
        }

        /// <summary>Setzt alle Nährwert-Felder auf realistische Werte (z.B. Mehl).</summary>
        public NährwertTestDataBuilder WithStandardMehlValues()
        {
            nährwert.Kalorien = 364;
            nährwert.Fett = 1.3;
            nährwert.GesättigteFettsäuren = 0.2;
            nährwert.Kohlenhydrate = 77;
            nährwert.Zucker = 0.3;
            nährwert.Protein = 10;
            nährwert.Ballaststoffe = 2.7;
            nährwert.Salz = 0.01;
            return this;
        }

        /// <summary>Setzt alle Nährwert-Felder auf realistische Werte (z.B. Milch).</summary>
        public NährwertTestDataBuilder WithStandardMilchValues()
        {
            nährwert.Kalorien = 61;
            nährwert.Fett = 3.25;
            nährwert.GesättigteFettsäuren = 1.86;
            nährwert.Kohlenhydrate = 4.8;
            nährwert.Zucker = 4.8;
            nährwert.Protein = 3.2;
            nährwert.Ballaststoffe = 0;
            nährwert.Salz = 0.044;
            return this;
        }

        /// <summary>Setzt die Standard-Mengen-Einheit (g oder ml).</summary>
        public NährwertTestDataBuilder WithStandardMengeEinheit(string einheit)
        {
            nährwert.StandardMengeEinheit = einheit;
            return this;
        }

        /// <summary>Setzt das Erstellungs-Datum.</summary>
        public NährwertTestDataBuilder WithCreatedAt(DateTime datum)
        {
            nährwert.CreatedAt = datum;
            return this;
        }

        /// <summary>Erstellt den Nährwert mit Standard-Werten (wenn nicht gesetzt).</summary>
        public Nährwert Build()
        {
            if (nährwert.LebensmittelId == 0)
                nährwert.LebensmittelId = 1;

            if (nährwert.Kalorien == 0 && nährwert.Fett == 0)
                WithStandardMehlValues();

            if (string.IsNullOrEmpty(nährwert.StandardMengeEinheit))
                nährwert.StandardMengeEinheit = "g";

            return nährwert;
        }

        /// <summary>Erstellt einen Nährwert für ein neues Lebensmittel (Id=0).</summary>
        public static Nährwert CreateNewNährwertForLebensmittel(int lebensmittelId)
        {
            return new NährwertTestDataBuilder()
                .WithId(0)
                .WithLebensmittelId(lebensmittelId)
                .WithStandardMehlValues()
                .WithStandardMengeEinheit("g")
                .Build();
        }
    }

    /// <summary>
    /// Statische Factory-Klasse für komfortable Erstellung von Test-Nährwerten.
    /// </summary>
    public static class NährwertTestDataBuilderFactory
    {
        /// <summary>Erstellt einen neuen Nährwert-Builder.</summary>
        public static NährwertTestDataBuilder CreateNährwert() => new NährwertTestDataBuilder();
    }
}

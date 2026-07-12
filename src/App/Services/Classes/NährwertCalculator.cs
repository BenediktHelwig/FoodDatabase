using System;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Exceptions;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Implementierung von INährwertCalculator für UC4 + UC5.
    /// Berechnet die Gesamt-Nährwerte und pro-Portion-Nährwerte für ein Rezept
    /// basierend auf den Zutaten und ihren Nährwertinformationen.
    /// </summary>
    public class NährwertCalculator : INährwertCalculator
    {
        private readonly IRezeptService _rezeptService;
        private readonly IRezeptZutatService _rezeptZutatService;
        private readonly INährwertService _nährwertService;
        private readonly IEinheitConverter _einheitConverter;

        public NährwertCalculator(
            IRezeptService rezeptService,
            IRezeptZutatService rezeptZutatService,
            INährwertService nährwertService,
            IEinheitConverter einheitConverter)
        {
            _rezeptService = rezeptService ?? throw new ArgumentNullException(nameof(rezeptService));
            _rezeptZutatService = rezeptZutatService ?? throw new ArgumentNullException(nameof(rezeptZutatService));
            _nährwertService = nährwertService ?? throw new ArgumentNullException(nameof(nährwertService));
            _einheitConverter = einheitConverter ?? throw new ArgumentNullException(nameof(einheitConverter));
        }

        /// <summary>
        /// Berechnet die Gesamt-Nährwerte und pro-Portion-Nährwerte für ein Rezept.
        /// </summary>
        public async Task<RezeptNährwerteDto> CalculateRezeptNährwerteAsync(int rezeptId)
        {
            // Prüfe ob Rezept existiert
            var rezept = await _rezeptService.GetRezeptByIdAsync(rezeptId);
            if (rezept is null)
            {
                throw new NotFoundException($"Rezept mit ID {rezeptId} nicht gefunden.");
            }

            // Hole alle Zutaten
            var zutaten = await _rezeptZutatService.GetZutatenAsync(rezeptId);

            // Berechne Gesamt-Nährwerte
            NährwerteDto gesamtNährwerte = new()
            {
                Kalorien = 0,
                Fett = 0,
                GesättigteFettsäuren = 0,
                Kohlenhydrate = 0,
                Zucker = 0,
                Protein = 0,
                Ballaststoffe = 0,
                Salz = 0
            };

            foreach (var zutat in zutaten)
            {
                // Hole Nährwertinformationen für diese Zutat
                var lebensmittelNährwert = await _nährwertService.GetNährwertByLebensmittelIdAsync(zutat.LebensmittelId);

                if (lebensmittelNährwert is null)
                    continue; // Ignoriere Zutaten ohne Nährwertinformationen

                // Konvertiere Menge zu Gramm
                double mengeInGramm = _einheitConverter.ConvertToGramm(zutat.Menge, zutat.Einheit);

                // Berechne Skalierungsfaktor (Menge / 100g)
                double skalierungsfaktor = mengeInGramm / 100.0;

                // Addiere die Nährwerte (Lebensmittel-Nährwert * Skalierungsfaktor)
                gesamtNährwerte.Kalorien += (int)Math.Round(lebensmittelNährwert.Kalorien * skalierungsfaktor);
                gesamtNährwerte.Fett += RoundToDecimalPlace(lebensmittelNährwert.Fett * skalierungsfaktor, 1);
                gesamtNährwerte.GesättigteFettsäuren += RoundToDecimalPlace(lebensmittelNährwert.GesättigteFettsäuren * skalierungsfaktor, 1);
                gesamtNährwerte.Kohlenhydrate += RoundToDecimalPlace(lebensmittelNährwert.Kohlenhydrate * skalierungsfaktor, 1);
                gesamtNährwerte.Zucker += RoundToDecimalPlace(lebensmittelNährwert.Zucker * skalierungsfaktor, 1);
                gesamtNährwerte.Protein += RoundToDecimalPlace(lebensmittelNährwert.Protein * skalierungsfaktor, 1);
                gesamtNährwerte.Ballaststoffe += RoundToDecimalPlace(lebensmittelNährwert.Ballaststoffe * skalierungsfaktor, 1);
                gesamtNährwerte.Salz += RoundToDecimalPlace(lebensmittelNährwert.Salz * skalierungsfaktor, 1);
            }

            // Berechne pro-Portion-Nährwerte
            NährwerteDto proPortionNährwerte = new()
            {
                Kalorien = (int)Math.Round((double)gesamtNährwerte.Kalorien / rezept.Portionen),
                Fett = RoundToDecimalPlace(gesamtNährwerte.Fett / rezept.Portionen, 1),
                GesättigteFettsäuren = RoundToDecimalPlace(gesamtNährwerte.GesättigteFettsäuren / rezept.Portionen, 1),
                Kohlenhydrate = RoundToDecimalPlace(gesamtNährwerte.Kohlenhydrate / rezept.Portionen, 1),
                Zucker = RoundToDecimalPlace(gesamtNährwerte.Zucker / rezept.Portionen, 1),
                Protein = RoundToDecimalPlace(gesamtNährwerte.Protein / rezept.Portionen, 1),
                Ballaststoffe = RoundToDecimalPlace(gesamtNährwerte.Ballaststoffe / rezept.Portionen, 1),
                Salz = RoundToDecimalPlace(gesamtNährwerte.Salz / rezept.Portionen, 1)
            };

            return new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = gesamtNährwerte,
                ProPortionNährwerteDto = proPortionNährwerte
            };
        }

        /// <summary>
        /// Berechnet die Nährwerte pro Portion für ein Rezept.
        /// </summary>
        public async Task<NährwerteProPortion> CalculateProPortionAsync(int rezeptId)
        {
            var nährwerte = await CalculateRezeptNährwerteAsync(rezeptId);

            return new NährwerteProPortion
            {
                Kalorien = nährwerte.ProPortionNährwerteDto.Kalorien,
                Fett = nährwerte.ProPortionNährwerteDto.Fett
            };
        }

        // ============ Hilfsmethoden ============

        /// <summary>
        /// Rundet einen Dezimalwert auf eine bestimmte Anzahl von Dezimalstellen.
        /// </summary>
        private double RoundToDecimalPlace(double value, int decimalPlaces)
        {
            return Math.Round(value, decimalPlaces);
        }
    }
}

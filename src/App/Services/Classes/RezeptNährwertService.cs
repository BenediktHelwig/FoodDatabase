using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Exceptions;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Implementierung von IRezeptNährwertService für UC5 (Nährwert-Berechnung für Rezepte).
    /// Bietet Methoden zur Berechnung und Anpassung von Nährwerten für Rezepte.
    /// </summary>
    public class RezeptNährwertService : IRezeptNährwertService
    {
        private readonly INährwertCalculator _nährwertCalculator;
        private readonly IRezeptService _rezeptService;
        private readonly IRezeptZutatService _rezeptZutatService;
        private readonly IEinheitConverter _einheitConverter;

        public RezeptNährwertService(
            INährwertCalculator nährwertCalculator,
            IRezeptService rezeptService,
            IRezeptZutatService rezeptZutatService,
            IEinheitConverter einheitConverter)
        {
            _nährwertCalculator = nährwertCalculator ?? throw new ArgumentNullException(nameof(nährwertCalculator));
            _rezeptService = rezeptService ?? throw new ArgumentNullException(nameof(rezeptService));
            _rezeptZutatService = rezeptZutatService ?? throw new ArgumentNullException(nameof(rezeptZutatService));
            _einheitConverter = einheitConverter ?? throw new ArgumentNullException(nameof(einheitConverter));
        }

        /// <summary>
        /// Ruft die Gesamt- und Pro-Portion-Nährwerte für ein Rezept ab.
        /// Delegiert die Berechnung an INährwertCalculator.
        /// </summary>
        public async Task<RezeptNährwerteDto> GetRezeptNährwerteAsync(int rezeptId)
        {
            try
            {
                return await _nährwertCalculator.CalculateRezeptNährwerteAsync(rezeptId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler bei der Berechnung der Nährwerte für Rezept {rezeptId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Berechnet Nährwerte direkt aus einer Liste von Zutaten.
        /// Nutzt den EinheitConverter für Konvertierungen.
        /// </summary>
        public async Task<NährwerteDto> CalculateNährwerteFromZutatenAsync(IEnumerable<RezeptZutat> zutaten, int portionen)
        {
            if (zutaten is null)
            {
                throw new ArgumentNullException(nameof(zutaten));
            }

            if (portionen <= 0)
            {
                throw new ArgumentException("Portionen müssen größer als 0 sein.", nameof(portionen));
            }

            var zutatenliste = zutaten.ToList();

            // Wenn keine Zutaten vorhanden, gebe Null-Nährwerte zurück
            if (!zutatenliste.Any())
            {
                return new NährwerteDto
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
            }

            // Berechne Gesamt-Nährwerte (ohne Portionierung)
            // Diese Methode funktioniert mit den Zutaten direkt
            // Annahme: Zutaten haben Navigation Properties zu Lebensmittel und Nährwert
            var gesamtNährwerte = new NährwerteDto
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

            foreach (var zutat in zutatenliste)
            {
                Nährwert lebensmittelNährwert = zutat.Lebensmittel?.Nährwert;
                if (lebensmittelNährwert is null)
                    continue;

                // Konvertiere Menge zu Gramm
                double mengeInGramm = _einheitConverter.ConvertToGramm(zutat.Menge, zutat.Einheit);

                // Berechne Skalierungsfaktor (Menge / 100g)
                double skalierungsfaktor = mengeInGramm / 100.0;

                // Addiere die Nährwerte
                gesamtNährwerte.Kalorien += (int)Math.Round(lebensmittelNährwert.Kalorien * skalierungsfaktor);
                gesamtNährwerte.Fett += RoundToDecimalPlace(lebensmittelNährwert.Fett * skalierungsfaktor, 1);
                gesamtNährwerte.GesättigteFettsäuren += RoundToDecimalPlace(lebensmittelNährwert.GesättigteFettsäuren * skalierungsfaktor, 1);
                gesamtNährwerte.Kohlenhydrate += RoundToDecimalPlace(lebensmittelNährwert.Kohlenhydrate * skalierungsfaktor, 1);
                gesamtNährwerte.Zucker += RoundToDecimalPlace(lebensmittelNährwert.Zucker * skalierungsfaktor, 1);
                gesamtNährwerte.Protein += RoundToDecimalPlace(lebensmittelNährwert.Protein * skalierungsfaktor, 1);
                gesamtNährwerte.Ballaststoffe += RoundToDecimalPlace(lebensmittelNährwert.Ballaststoffe * skalierungsfaktor, 1);
                gesamtNährwerte.Salz += RoundToDecimalPlace(lebensmittelNährwert.Salz * skalierungsfaktor, 1);
            }

            return await Task.FromResult(gesamtNährwerte);
        }

        /// <summary>
        /// Passt die Nährwerte eines Rezepts für eine neue Portionszahl an.
        /// </summary>
        public async Task<RezeptNährwerteDto> AdjustNährwerteForPortionAsync(int rezeptId, int newPortionen)
        {
            if (newPortionen <= 0)
            {
                throw new ArgumentException("Neue Portionszahl muss größer als 0 sein.", nameof(newPortionen));
            }

            // Hole das Rezept
            var rezept = await _rezeptService.GetRezeptByIdAsync(rezeptId);
            if (rezept is null)
            {
                throw new NotFoundException($"Rezept mit ID {rezeptId} nicht gefunden.");
            }

            // Hole die aktuellen Nährwerte
            var aktuelleNährwerte = await GetRezeptNährwerteAsync(rezeptId);

            // Berechne Skalierungsfaktor: newPortionen / aktuellePortionen
            double skalierungsfaktor = (double)newPortionen / rezept.Portionen;

            // Skaliere die Gesamt-Nährwerte
            var angepassteGesamtnährwerte = new NährwerteDto
            {
                Kalorien = (int)Math.Round(aktuelleNährwerte.GesamtnährwerteDto.Kalorien * skalierungsfaktor),
                Fett = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Fett * skalierungsfaktor, 1),
                GesättigteFettsäuren = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.GesättigteFettsäuren * skalierungsfaktor, 1),
                Kohlenhydrate = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Kohlenhydrate * skalierungsfaktor, 1),
                Zucker = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Zucker * skalierungsfaktor, 1),
                Protein = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Protein * skalierungsfaktor, 1),
                Ballaststoffe = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Ballaststoffe * skalierungsfaktor, 1),
                Salz = RoundToDecimalPlace(aktuelleNährwerte.GesamtnährwerteDto.Salz * skalierungsfaktor, 1)
            };

            // Pro-Portion-Nährwerte bleiben gleich (werden immer basierend auf 1 Portion berechnet)
            // Aber wir skalieren sie auch für die neue Portionszahl
            var angepassteProPortionNährwerte = new NährwerteDto
            {
                Kalorien = (int)Math.Round(angepassteGesamtnährwerte.Kalorien / (double)newPortionen),
                Fett = RoundToDecimalPlace(angepassteGesamtnährwerte.Fett / newPortionen, 1),
                GesättigteFettsäuren = RoundToDecimalPlace(angepassteGesamtnährwerte.GesättigteFettsäuren / newPortionen, 1),
                Kohlenhydrate = RoundToDecimalPlace(angepassteGesamtnährwerte.Kohlenhydrate / newPortionen, 1),
                Zucker = RoundToDecimalPlace(angepassteGesamtnährwerte.Zucker / newPortionen, 1),
                Protein = RoundToDecimalPlace(angepassteGesamtnährwerte.Protein / newPortionen, 1),
                Ballaststoffe = RoundToDecimalPlace(angepassteGesamtnährwerte.Ballaststoffe / newPortionen, 1),
                Salz = RoundToDecimalPlace(angepassteGesamtnährwerte.Salz / newPortionen, 1)
            };

            return new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = angepassteGesamtnährwerte,
                ProPortionNährwerteDto = angepassteProPortionNährwerte
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

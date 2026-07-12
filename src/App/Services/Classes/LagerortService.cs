using FoodDatabase.App.Data;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Lagerverwaltung (UC9).
    /// Verwaltet dynamische Lagerorte mit Validierung, Normalisierung und Auto-Complete.
    /// </summary>
    public class LagerortService : ILagerortService
    {
        private readonly FoodDatabaseContext _context;

        /// <summary>Initialisiert eine neue Instanz des LagerortService.</summary>
        /// <param name="context">Der EF Core DbContext für Datenbankzugriff.</param>
        public LagerortService(FoodDatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft alle nicht archivierten Lagerorte ab.
        /// </summary>
        public async Task<IEnumerable<Lagerort>> GetAlleLagerorte()
        {
            return await _context.Lagerorte
                .Where(l => !l.IsArchived)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Sucht Lagerorte mit Fuzzy-Matching (Partial Prefix, Case-Insensitive).
        /// Wird für Auto-Complete-Funktionalität verwendet.
        /// </summary>
        public async Task<IEnumerable<Lagerort>> GetLagerorteMitAutoComplete(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return await GetAlleLagerorte();
            }

            var lowerPrefix = prefix.ToLower();

            return await _context.Lagerorte
                .Where(l => !l.IsArchived && l.Name.ToLower().Contains(lowerPrefix))
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Validiert einen Lagornamen (nur Buchstaben A-Z, a-z).
        /// </summary>
        public void ValidateLagerort(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Lagerortname darf nicht null oder leer sein.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Lagerortname darf nicht nur Whitespace sein.", nameof(name));
            }

            // Nur Buchstaben erlaubt (A-Z, a-z)
            if (!Regex.IsMatch(name, @"^[A-Za-z]+$"))
            {
                throw new ArgumentException("Nur Buchstaben (A-Z, a-z) erlaubt.", nameof(name));
            }
        }

        /// <summary>
        /// Normalisiert einen Lagornamen (Single-Word mit intelligenter Case-Behandlung).
        /// Erkenne Lower-to-Upper Übergänge: "lagerA" → "LagerA", "LAGER" → "Lager", "LagerB" → "LagerB".
        /// Nur für Single-Word-Input. Multi-Word wird nicht unterstützt (Validierung blockiert Leerzeichen).
        /// </summary>
        public string NormalisiereLagerort(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Zähle Lower-to-Upper Übergänge und Großbuchstaben
            int lowerToUpperTransitions = 0;
            int upperCaseCount = 0;

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    upperCaseCount++;
                }

                if (char.IsLower(input[i - 1]) && char.IsUpper(input[i]))
                {
                    lowerToUpperTransitions++;
                }
            }

            // Wenn Input mit Großbuchstabe startet
            if (char.IsUpper(input[0]))
            {
                upperCaseCount++;
            }

            // Intelligente Regel: Behalte Pattern nur bei genau 1 Übergang und 1-2 Großbuchstaben
            if (lowerToUpperTransitions == 1 && upperCaseCount <= 2)
            {
                // Behalte Pattern: Erkenne Lower-to-Upper Übergänge
                System.Text.StringBuilder result = new();
                result.Append(char.ToUpper(input[0]));

                for (int i = 1; i < input.Length; i++)
                {
                    if (char.IsLower(input[i - 1]) && char.IsUpper(input[i]))
                    {
                        result.Append(char.ToUpper(input[i]));
                    }
                    else
                    {
                        result.Append(char.ToLower(input[i]));
                    }
                }

                return result.ToString();
            }

            // Sonst: Simple Capitalize (First Upper, Rest Lower)
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        /// <summary>
        /// Erstellt einen neuen Lagerort oder gibt den existierenden zurück (GetOrCreate-Pattern).
        /// Normalisiert den Namen vor Verarbeitung. Verhindert Duplikate durch DB UNIQUE Index.
        /// </summary>
        public async Task<Lagerort> GetOrCreateAsync(string name)
        {
            // Validierung zuerst
            ValidateLagerort(name);

            // Normalisierung
            var normalizedName = NormalisiereLagerort(name);

            // Prüfe ob bereits vorhanden (Case-Sensitive wegen DB-Index)
            var existing = await _context.Lagerorte
                .FirstOrDefaultAsync(l => l.Name == normalizedName);

            if (existing is not null)
            {
                return existing;
            }

            // Neu anlegen
            Lagerort newLagerort = new()
            {
                Name = normalizedName,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            };

            _context.Lagerorte.Add(newLagerort);
            await _context.SaveChangesAsync();

            return newLagerort;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// UC8: Service für Einkaufslistengeneration.
    /// Generiert eine flache Liste (ein Eintrag pro Lebensmittel) mit Lebensmitteln,
    /// deren Gesamtbestand den Mindestbestand erreicht/unterschreitet.
    /// </summary>
    public class EinkaufslistenService : IEinkaufslistenService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        /// <summary>
        /// Initialisiert eine neue Instanz des EinkaufslistenService.
        /// </summary>
        /// <param name="repository">Repository für ProduktInstanz-Persistierung.</param>
        public EinkaufslistenService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Generiert eine Einkaufsliste für Lebensmittel, deren Gesamtbestand
        /// den Mindestbestand erreicht oder unterschreitet (<= Trigger).
        /// Aggregiert über alle ProduktInstanzen pro LebensmittelKatalogId.
        /// </summary>
        /// <returns>
        /// Liste von EinkaufslistenEintragDto, eine pro unterschrittenem Lebensmittel,
        /// sortiert nach LebensmittelName. Leere Liste, wenn alle Bestände OK oder keine Instanzen.
        /// </returns>
        public async Task<List<EinkaufslistenEintragDto>> GetEinkaufslisteAsync()
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                return new List<EinkaufslistenEintragDto>();

            var einkaufsliste = allInstanzen
                .GroupBy(x => x.LebensmittelKatalogId)
                .Select(g => new
                {
                    LebensmittelId = g.Key,
                    Gesamtmenge = g.Sum(x => x.Menge),
                    Mindestbestand = g.FirstOrDefault()?.MindestbestandMenge ?? 0,
                    ErsteInstanz = g.FirstOrDefault()
                })
                .Where(x => x.Gesamtmenge <= x.Mindestbestand)
                .Select(x => new EinkaufslistenEintragDto
                {
                    LebensmittelKatalogId = x.LebensmittelId,
                    LebensmittelName = x.ErsteInstanz?.LebensmittelKatalog?.Name ?? $"Lebensmittel #{x.LebensmittelId}",
                    AktuelleGesamtmenge = x.Gesamtmenge,
                    MindestbestandMenge = x.Mindestbestand,
                    Fehlmenge = x.Mindestbestand - x.Gesamtmenge,
                    Einheit = x.ErsteInstanz?.LebensmittelKatalog?.Einheit ?? string.Empty
                })
                .OrderBy(x => x.LebensmittelName)
                .ToList();

            return einkaufsliste;
        }
    }
}

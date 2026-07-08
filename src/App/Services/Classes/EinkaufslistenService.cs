using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }
    }
}

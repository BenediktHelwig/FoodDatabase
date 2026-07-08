using FoodDatabase.App.Services.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// UC8: Service für Einkaufslistengeneration basierend auf Mindestbestand.
    /// Generiert eine flache Liste mit einem Eintrag pro Lebensmittel.
    /// Trigger: Gesamtmenge <= MindestbestandMenge (on-demand berechnet, nicht persistiert).
    /// </summary>
    public interface IEinkaufslistenService
    {
        /// <summary>
        /// Generiert eine Einkaufsliste für Lebensmittel, deren Gesamtbestand
        /// den Mindestbestand erreicht oder unterschreitet (<=).
        /// Aggregiert über alle ProduktInstanzen pro LebensmittelKatalogId.
        /// </summary>
        /// <returns>
        /// Liste von EinkaufslistenEintragDto, eine pro unterschrittenem Lebensmittel,
        /// sortiert nach LebensmittelName. Leere Liste, wenn alle Bestände OK oder keine Instanzen.
        /// </returns>
        Task<List<EinkaufslistenEintragDto>> GetEinkaufslisteAsync();
    }
}

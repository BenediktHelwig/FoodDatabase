using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Produktinstanz-Verwaltung (UC10).
    /// Eine ProduktInstanz ist eine einzelne Packung mit eigenem Verfallsdatum und Lagerort.
    /// Bietet CRUD-Operationen und Abfragen (nach Lebensmittel, Lagerort, Verfallsdatum).
    /// </summary>
    public interface IProduktInstanzService
    {
        /// <summary>Erstellt eine neue ProduktInstanz mit Validierungen.</summary>
        Task<ProduktInstanz> CreateAsync(int lebensmittelKatalogId, decimal menge, DateTime verfallsdatum, string lagerort);

        /// <summary>Ruft eine ProduktInstanz anhand ihrer ID ab.</summary>
        Task<ProduktInstanz> GetByIdAsync(int id);

        /// <summary>Ruft alle ProduktInstanzen für ein Lebensmittel ab.</summary>
        Task<List<ProduktInstanz>> GetByLebensmittelAsync(int lebensmittelKatalogId);

        /// <summary>Ruft alle ProduktInstanzen für einen Lagerort ab.</summary>
        Task<List<ProduktInstanz>> GetByLagerortAsync(string lagerort);

        /// <summary>Ruft alle Instanzen sortiert nach Verfallsdatum ab (FIFO-Reihenfolge: älteste zuerst).</summary>
        Task<List<ProduktInstanz>> GetNachVerfallsdatumSortiertAsync();

        /// <summary>Ruft alle abgelaufenen Instanzen ab (Verfallsdatum <= Standdatum).</summary>
        Task<List<ProduktInstanz>> GetVerfallenenAsync(DateTime? standdatum = null);

        /// <summary>Aktualisiert eine bestehende ProduktInstanz mit Validierungen.</summary>
        Task UpdateAsync(int id, decimal menge, DateTime verfallsdatum, string lagerort);

        /// <summary>Löscht eine ProduktInstanz anhand ihrer ID.</summary>
        Task DeleteAsync(int id);

        /// <summary>Berechnet die Tage bis zum Verfallsdatum einer Instanz. Negative Werte = bereits abgelaufen.</summary>
        Task<int> GetTagesBisVerfallAsync(int id);
    }
}

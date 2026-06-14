using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    public interface IProduktInstanzService
    {
        Task<ProduktInstanz> CreateAsync(int lebensmittelKatalogId, decimal menge, DateTime verfallsdatum, string lagerort);
        Task<ProduktInstanz> GetByIdAsync(int id);
        Task<List<ProduktInstanz>> GetByLebensmittelAsync(int lebensmittelKatalogId);
        Task<List<ProduktInstanz>> GetByLagerortAsync(string lagerort);
        Task<List<ProduktInstanz>> GetNachVerfallsdatumSortiertAsync();
        Task<List<ProduktInstanz>> GetVerfallenenAsync(DateTime? standdatum = null);
        Task UpdateAsync(int id, decimal menge, DateTime verfallsdatum, string lagerort);
        Task DeleteAsync(int id);
        Task<int> GetTagesBisVerfallAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    public interface ILagerbestandService
    {
        Task<decimal> GetGesamtmengeAsync(int lebensmittelKatalogId);
        Task<bool> CheckMindestbestandUnterschrittenAsync(int lebensmittelKatalogId);
        Task<List<ProduktInstanz>> GetEinkaufslistenEintraegeAsync();
        Task<List<ProduktInstanz>> GetBestaendePorLagerortAsync(string lagerort);
        Task<ProduktInstanz> AddToBestandAsync(int lebensmittelKatalogId, decimal menge, decimal mindestbestandMenge, DateTime verfallsdatum, string lagerort);
        Task<ProduktInstanz> UpdateMengeAsync(int id, decimal newMenge);
        Task RemoveFromBestandAsync(int id);
        Task<bool> ValidateBestandAsync(int id, decimal requiredMenge);
    }
}

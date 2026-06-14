using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    public interface IProduktInstanzService
    {
        Task<ProduktInstanz> CreateProduktInstanzAsync(ProduktInstanz instanz);
        Task<ProduktInstanz> GetProduktInstanzByIdAsync(int id);
        Task<IEnumerable<ProduktInstanz>> GetAllProduktInstanzenAsync();
        Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenByLebensmittelIdAsync(int lebensmittelId);
        Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenByLagerortIdAsync(int lagerortId);
        Task<IEnumerable<ProduktInstanz>> GetAbgelaufeneProduktInstanzenAsync();
        Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenBaldAbgelaufen_Async(int tagesBis);
        Task<ProduktInstanz> UpdateProduktInstanzAsync(ProduktInstanz instanz);
        Task<ProduktInstanz> ReduceProduktInstanzMengeAsync(int id, double mengeZuReduzieren);
        Task<bool> DeleteProduktInstanzAsync(int id);
        bool IsUnterMindestbestand(ProduktInstanz instanz);
        bool IsAbgelaufen(ProduktInstanz instanz);
        int CalculateTagesBisAblauf(ProduktInstanz instanz);
    }
}

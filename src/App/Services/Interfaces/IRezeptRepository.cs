using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Spezialisiertes Repository-Interface für Rezepte.
    /// Nutzt CreateAsync statt AddAsync für konsistente Nomenklatur mit den Test-Mocks.
    /// </summary>
    public interface IRezeptRepository
    {
        Task<IEnumerable<Rezept>> GetAllAsync();
        Task<Rezept> GetByIdAsync(int id);
        Task<Rezept> CreateAsync(Rezept entity);
        Task<Rezept> UpdateAsync(Rezept entity);
        Task<bool> DeleteAsync(int id);
    }
}

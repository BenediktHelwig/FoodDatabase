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

    /// <summary>
    /// Spezialisiertes Repository-Interface für Rezept-Zutaten.
    /// </summary>
    public interface IRezeptZutatRepository
    {
        Task<IEnumerable<RezeptZutat>> GetAllAsync();
        Task<RezeptZutat> GetByIdAsync(int id);
        Task<RezeptZutat> CreateAsync(RezeptZutat entity);
        Task<RezeptZutat> UpdateAsync(RezeptZutat entity);
        Task<bool> DeleteAsync(int id);
    }
}

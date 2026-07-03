using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
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
